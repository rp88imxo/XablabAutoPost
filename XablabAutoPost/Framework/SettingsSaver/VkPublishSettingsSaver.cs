using Newtonsoft.Json;
using XablabAutoPost.Core.Saver;

namespace XablabAutoPost.Framework.SettingsSaver;

public class VkPublishSettings
{
    public VkPublishSettings(string accessToken, ulong groupId, string username, string password, ulong applicationId, string code)
    {
        AccessToken = accessToken;
        GroupId = groupId;
        Username = username;
        Password = password;
        ApplicationId = applicationId;
        Code = code;
    }

    [JsonProperty("access_token")] 
    public string AccessToken { get; set; }
    
    [JsonProperty("application_id")] 
    public ulong ApplicationId { get; set; }
    
    [JsonProperty("username")] 
    public string Username { get; set; }
     
    [JsonProperty("password")] 
    public string Password { get; set; }
    
    [JsonProperty("group_id")]
    public ulong GroupId { get; set; }
    
    [JsonProperty("code")] 
    public string Code { get; set; }
}

public class VkPublishSettingsSaver : Saver<VkPublishSettings>
{
    protected override string DirectoryName => "VkPublishSettings";
    protected override string FileName => "VkPublishSettings";
    
    public VkPublishSettings LoadSettings()
    {
        var data = Load()
                   ?? new VkPublishSettings("", 207932245, "USERNAME", "PASSWORD", 0, "CODE");

        Save(data);

        return data;
    }
}