using System.Net;
using System.Net.Http.Headers;
using System.Text;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;
using XablabAutoPost.Core.ConsoleLogger;
using XablabAutoPost.Core.PostCreator;
using XablabAutoPost.Framework.SettingsSaver;

namespace XablabAutoPost.Framework.PostPublishers;

public class VkPostPublisher : IPostPublisher
{
    private readonly ApplicationPersistentProvider _applicationPersistentProvider;
    private readonly UsedPostsData _usedPosts;
    private readonly ApplicationSettings _settings;
    private VkApi _api;
    private VkPublishSettings _vkPublishSettings;
    private readonly WebClient _wc;

    public VkPostPublisher(ApplicationPersistentProvider applicationPersistentProvider)
    {
        _applicationPersistentProvider = applicationPersistentProvider;
        _usedPosts = _applicationPersistentProvider.UsedPostsSaver.LoadUsedPosts();
        _settings = _applicationPersistentProvider.SettingsSaver.LoadSettings();
        _vkPublishSettings = _applicationPersistentProvider.VkPublishSettingsSaver.LoadSettings();

        _wc = new WebClient();

        InitVkApi();
    }

    private void InitVkApi()
    {
        _api = new VkApi();
        
        _api.Authorize(new ApiAuthParams
        {
            ApplicationId = _vkPublishSettings.ApplicationId,
            Login = _vkPublishSettings.Username,
            Password = _vkPublishSettings.Password,
            Settings = Settings.All,
            TwoFactorAuthorization = TwoFactorAuthorization,
        });
        
        string TwoFactorAuthorization()
        {
            Console.Write("Enter a verification code: ");
            var code = Console.ReadLine();
            return code!;
        }
    }

    public async Task PublishPostsAsync(IList<PostEntry> postEntries)
    {
        for (var index = 0; index < postEntries.Count; index++)
        {
            if (index + 1 > _settings.PostsToPublish)
            {
                return;
            }

            var postEntry = postEntries[index];
            await PublishPostAsync(postEntry);
        }
    }

    public async Task PublishPostAsync(PostEntry postEntry)
    {
        await Task.Delay(200);

        try
        {
            if (!_usedPosts.UsedPostsIds.Contains(postEntry.PostId))
            {
                _usedPosts.UsedPostsIds.Add(postEntry.PostId);
            }

            ulong groupId = _vkPublishSettings.GroupId;

            var postPhotoAttachment = await CreatePostPhotoAttachmentAsync(groupId, postEntry);

            var fileInf = new FileInfo(postEntry.FilePath);

            MediaAttachment? modelAttachment = null;

            if (fileInf.Exists && (fileInf.Length / 1048576.0) < 100)
            {
                modelAttachment = await CreateModelAttachmentAsync(groupId, postEntry);
            }

            var attachments = new List<MediaAttachment>();

            if (modelAttachment != null)
            {
                attachments.Add(modelAttachment);
            }

            if (postPhotoAttachment != null)
            {
                attachments.AddRange(postPhotoAttachment);
            }

            string messageToGroup = BuildPostMessage(postEntry, modelAttachment);

            var wallPostParams = new WallPostParams()
            {
                OwnerId = -(long)groupId,
                FromGroup = true,
                Message = messageToGroup,
                Attachments = postPhotoAttachment,
            };

            _api.Wall.Post(wallPostParams);

            _applicationPersistentProvider.UsedPostsSaver.Save(_usedPosts);

            ConsoleLogger.Log("Vk Post Publish", $"published post {postEntry.PostName} with id {postEntry.PostId}",
                ConsoleColor.Green);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private string BuildPostMessage(PostEntry postEntry, MediaAttachment? modelAttachment)
    {
        var stringBuilder = new StringBuilder(128);
        stringBuilder.Append($"❗Наша новейшая модель: {postEntry.PostName}. ❗" +
                             $"\nУзнать стоимость? переходи по ссылке xablab.ru/upload" +
                             $"\n#ХабЛаб #3D_печать");

        if (modelAttachment != null)
        {
            stringBuilder.Append("\n↘ Файл с моделью во вложении ↙");
        }
        else
        {
            stringBuilder.Append("\nФайл с моделью доступен по ссылке" +
                                 $"\n\u25b6{postEntry.PostUri}\u25c0");
        }

        return stringBuilder.ToString();
    }

    private async Task<MediaAttachment?> CreateModelAttachmentAsync(ulong groupId, PostEntry postEntry)
    {
        var uploadServer = _api.Docs.GetWallUploadServer((long)groupId);

        var response = await UploadFile(uploadServer.UploadUrl,
            postEntry.FilePath, Path.GetExtension(postEntry.FilePath));

        return _api.Docs.Save(response, postEntry.PostName)[0].Instance;
    }

    private async Task<IReadOnlyCollection<Photo>?> CreatePostPhotoAttachmentAsync(ulong groupId, PostEntry postEntry)
    {
        var uploadServer = _api.Photo.GetWallUploadServer((long)groupId);

        var result = await _wc.UploadFileTaskAsync(uploadServer.UploadUrl, postEntry.MainImagePath);
        var responseImg = Encoding.ASCII.GetString(result);

        var attachments = _api.Photo.SaveWallPhoto(responseImg, null, groupId);

        return attachments;
    }

    private async Task<string> UploadFile(string serverUrl, string file, string fileExtension)
    {
        // Получение массива байтов из файла
        var data = GetBytes(file);

        // Создание запроса на загрузку файла на сервер
        using (var client = new HttpClient())
        {
            var requestContent = new MultipartFormDataContent();
            var content = new ByteArrayContent(data);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            requestContent.Add(content, "file", $"file.{fileExtension}");

            var response = client.PostAsync(serverUrl, requestContent).Result;
            return Encoding.Default.GetString(await response.Content.ReadAsByteArrayAsync());
        }
    }

    private byte[] GetBytes(string filePath)
    {
        return File.ReadAllBytes(filePath);
    }
}