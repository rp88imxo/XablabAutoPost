using System.Net.Http.Headers;
using System.Text;
using VkNet;
using VkNet.Model;
using XablabAutoPost.Core.PostCreator;
using XablabAutoPost.Framework.SettingsSaver;

namespace XablabAutoPost.Framework.PostPublishers;

public class VkPostPublisher : IPostPublisher
{
    private readonly ApplicationPersistentProvider _applicationPersistentProvider;
    private readonly UsedPostsData _usedPosts;
    private readonly ApplicationSettings _settings;
    private VkApi _api;
    private readonly VkPublishSettings _vkPublishSettings;

    public VkPostPublisher(ApplicationPersistentProvider applicationPersistentProvider)
    {
        _applicationPersistentProvider = applicationPersistentProvider;
        _usedPosts = _applicationPersistentProvider.UsedPostsSaver.LoadUsedPosts();
        _settings = _applicationPersistentProvider.SettingsSaver.LoadSettings();
        _vkPublishSettings = _applicationPersistentProvider.VkPublishSettingsSaver.LoadSettings();

        InitVkApi();
    }

    private void InitVkApi()
    {
        _api = new VkApi();
        
        _api.Authorize(new ApiAuthParams
        {
            AccessToken = _vkPublishSettings.AccessToken,
        });
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

        if (!_usedPosts.UsedPostsIds.Contains(postEntry.PostId))
        {
            _usedPosts.UsedPostsIds.Add(postEntry.PostId);
        }
        
        _applicationPersistentProvider.UsedPostsSaver.Save(_usedPosts);

        ulong groupId = _vkPublishSettings.GroupId;
        
        #region UPLOAD_ZIP_TO_VK_SERVER

        //var uploadServer = _api.Photo.GetWallUploadServer((long)groupId);
        var uploadServer = _api.Docs.GetWallUploadServer((long)groupId);
       //var uploadServer = _api.Photo.getuploadserv((long)groupId);

       var response = await UploadFile(uploadServer.UploadUrl,
           postEntry.FilePath, Path.GetExtension(postEntry.FilePath));
        
       var attachments = _api.Docs.Save(response, postEntry.PostName);
       
        // var uploadServer = _api.Docs.GetWallUploadServer((long)groupId);
        //
        // // Загрузить файл на сервер VK.
        // var response = await UploadFile(uploadServer.UploadUrl,
        //     postEntry.MainImagePath, Path.GetExtension(postEntry.MainImagePath));
        //
        // var attachments = new List<MediaAttachment>()
        // {
        //     _api.Docs.Save(response, postEntry.PostName)[0].Instance,
        // };
        
        #endregion

        string messageToGroup = $"❗Наша новейшая модель: {postEntry.PostName}. ❗" +
                                $"\nУзнать стоимость? переходи по ссылке xablab.ru/upload" +
                                $"\n#ХабЛаб #3D_печать" +
                                $"\n↘ Файл с моделью во вложении ↙";
        
        var wallPostParams = new WallPostParams()
        {
            OwnerId = -(long)groupId,
            FromGroup = true,
            Message = messageToGroup,
            Attachments = attachments.Select(x=> x.Instance),
        };
            
        _api.Wall.Post(wallPostParams);
        
        var oldColor = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"[Vk Post Publish]: published post {postEntry.PostName} with id {postEntry.PostId}");
        Console.ForegroundColor = oldColor;
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