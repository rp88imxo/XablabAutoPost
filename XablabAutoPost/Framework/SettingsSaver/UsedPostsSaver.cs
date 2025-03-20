using Newtonsoft.Json;
using XablabAutoPost.Core.Saver;

namespace XablabAutoPost.Framework.SettingsSaver;

public class UsedPostsData
{
    public UsedPostsData(List<string> usedPostsIds)
    {
        UsedPostsIds = usedPostsIds;
    }

    [JsonProperty("used_posts_ids")]
    public List<string> UsedPostsIds { get; set; }
}

public class UsedPostsSaver : Saver<UsedPostsData>
{
    protected override string DirectoryName => "UsedPostsData";
    protected override string FileName => "UsedPostsData";

    public UsedPostsData LoadUsedPosts()
    {
        var data = Load()
                   ?? new UsedPostsData(new List<string>(){"23232", "252"});

        Save(data);

        return data;
    }
}