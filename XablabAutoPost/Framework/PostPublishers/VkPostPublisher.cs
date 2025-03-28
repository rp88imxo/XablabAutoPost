﻿using System.Net;
using System.Net.Http.Headers;
using System.Text;
using VkNet;
using VkNet.Enums.Filters;
using VkNet.Exception;
using VkNet.Model;
using XablabAutoPost.Core.ConsoleLogger;
using XablabAutoPost.Core.PostCreator;
using XablabAutoPost.Framework.Promocode;
using XablabAutoPost.Framework.SettingsSaver;
using XablabAutoPost.Framework.VK;

namespace XablabAutoPost.Framework.PostPublishers;

public class VkPostPublisher : IPostPublisher
{
    private readonly ApplicationPersistentProvider _applicationPersistentProvider;
    private readonly IDiscountProvider _discountProvider;
    private readonly UsedPostsData _usedPosts;
    private readonly ApplicationSettings _settings;
    private VkApi _api;
    private VkPublishSettings _vkPublishSettings;
    private readonly WebClient _wc;
    private readonly VkMessagesData _vkMessagesTemplatesData;

    public VkPostPublisher(ApplicationPersistentProvider applicationPersistentProvider, IDiscountProvider discountProvider)
    {
        _applicationPersistentProvider = applicationPersistentProvider;
        _discountProvider = discountProvider;
        _usedPosts = _applicationPersistentProvider.UsedPostsSaver.LoadUsedPosts();
        _settings = _applicationPersistentProvider.SettingsSaver.LoadSettings();
        _vkPublishSettings = _applicationPersistentProvider.VkPublishSettingsSaver.LoadSettings();
        _vkMessagesTemplatesData = _applicationPersistentProvider.VkMessagesTemplatesSaver.LoadMessagesTemplatesData();
        
        _wc = new WebClient();

        InitVkApi();
    }

    private void InitVkApi()
    {
        ConsoleLogger.Log("VKPostPublisher", "Initializing vk api...", ConsoleColor.Green);
        
        _api = new VkApi(null, new CapcthaSolver());

        Authorize();
        
        ConsoleLogger.Log("VKPostPublisher", "Initialized vk api successfully!", ConsoleColor.Green);
    }

    private void Authorize()
    {
        var token = _vkPublishSettings.AccessToken;

        if (!string.IsNullOrEmpty(token))
        {
            _api.Authorize(new ApiAuthParams
            {
                AccessToken = _vkPublishSettings.AccessToken,
            });
        }
        else
        {
            _api.Authorize(new ApiAuthParams
            {
                ApplicationId = _vkPublishSettings.ApplicationId,
                Login = _vkPublishSettings.Username,
                Password = _vkPublishSettings.Password,
                Settings = Settings.All,
                TwoFactorAuthorization = TwoFactorAuthorization,
            });
            
            _vkPublishSettings.AccessToken = _api.Token;
            _applicationPersistentProvider.VkPublishSettingsSaver.Save(_vkPublishSettings);
        }
        
        string TwoFactorAuthorization()
        {
            Console.Write("Enter a verification code: ");
            var code = Console.ReadLine();
            return code!;
        }
    }

    public async Task PublishPostsAsync(IList<PostEntry> postEntries)
    {
        int publishedCount = 0;
        int failCount = 0;
        for (var index = 0; index < postEntries.Count; index++)
        {
            if (publishedCount >= _settings.PostsToPublish)
            {
                return;
            }

            var postEntry = postEntries[index];
            var result = await PublishPostAsync(postEntry);
            if (result == PublishResult.Success)
            {
                publishedCount++;
            }
            else
            {
                failCount++;
            }
        }
        
        ConsoleLogger.Log("VkPostPublisher", $"Published post statistics: Successes count {publishedCount}, fail count {failCount}", ConsoleColor.Gray);
    }

    public async Task<PublishResult> PublishPostAsync(PostEntry postEntry)
    {
        await Task.Delay(200);

        try
        {
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
                Attachments = attachments,
            };

            _api.Wall.Post(wallPostParams);

            if (!_usedPosts.UsedPostsIds.Contains(postEntry.PostId))
            {
                _usedPosts.UsedPostsIds.Add(postEntry.PostId);
                _applicationPersistentProvider.UsedPostsSaver.Save(_usedPosts);
            }
            
            ConsoleLogger.Log("Vk Post Publish", $"Published post {postEntry.PostName} with id {postEntry.PostId}",
                ConsoleColor.Green);
            
            return PublishResult.Success;
        }
        catch (UserAuthorizationFailException userAuthorizationFailException)
        {
            ConsoleLogger.Log("Vk Post Publish",
                "Token has been expired! Trying to authorize again...",
                ConsoleColor.Red);
            
            _vkPublishSettings.AccessToken = string.Empty;
            Authorize();
            return PublishResult.Fail;
        }
        catch (Exception e)
        {
            ConsoleLogger.Log("Vk Post Publish",
                $"Failed to publish a post {postEntry.PostName} with id {postEntry.PostId}, error is {e.Message}, stacktrace {e.StackTrace}",
                ConsoleColor.Red);
            
            return PublishResult.Fail;
        }
    }

    private string BuildPostMessage(PostEntry postEntry, MediaAttachment? modelAttachment)
    {
        var stringBuilder = new StringBuilder(128);
        
        var messageTemplate = GetRandomMessageTemplate();
        
        var discountEntry = _discountProvider.GetDiscount();
        
        stringBuilder.AppendFormat(messageTemplate, postEntry.PostName, discountEntry.Value, discountEntry.Code);
        
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

    private string GetRandomMessageTemplate()
    {
        var totalCount = _vkMessagesTemplatesData.MessageDataTemplate.Count;
       var index = Random.Shared.Next(0, totalCount - 1);
       return _vkMessagesTemplatesData.MessageDataTemplate[index].Message;
    }

    private async Task<MediaAttachment?> CreateModelAttachmentAsync(ulong groupId, PostEntry postEntry)
    {
        var uploadServer = _api.Docs.GetWallUploadServer((long)groupId);

        var response = await UploadFile(uploadServer.UploadUrl,
            postEntry.FilePath, Path.GetExtension(postEntry.FilePath));

        var resultAttachments = _api.Docs.Save(response, postEntry.PostName);
        
        return resultAttachments.Count == 1 ? resultAttachments[0].Instance : null;
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