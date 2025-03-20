using Newtonsoft.Json;
using XablabAutoPost.Core.Saver;

namespace XablabAutoPost.Framework.SettingsSaver;

public class VkPublishSettings
{
    public VkPublishSettings(string accessToken, ulong groupId)
    {
        AccessToken = accessToken;
        GroupId = groupId;
    }

    [JsonProperty("access_token")] 
    public string AccessToken { get; set; }
    
    
    [JsonProperty("group_id")]
    public ulong GroupId { get; set; }
}

public class VkPublishSettingsSaver : Saver<VkPublishSettings>
{
    protected override string DirectoryName => "VkPublishSettings";
    protected override string FileName => "VkPublishSettings";
    
    public VkPublishSettings LoadSettings()
    {
        var data = Load()
                   ?? new VkPublishSettings("PASTE YOUR ACCESS TOKEN HERE", 207932245);

        Save(data);

        return data;
    }
}