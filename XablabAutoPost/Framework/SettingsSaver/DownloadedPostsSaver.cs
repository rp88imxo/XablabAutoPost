using Newtonsoft.Json;
using XablabAutoPost.Core.PostCreator;
using XablabAutoPost.Core.Saver;

namespace XablabAutoPost.Framework.SettingsSaver;

public class DownloadedPosts
{
    public DownloadedPosts(List<PostEntry> postEntries)
    {
        PostEntries = postEntries;
    }

    [JsonProperty("post_entries")]
    public List<PostEntry> PostEntries { get; set; }
}


public class DownloadedPostsSaver : Saver<DownloadedPosts>
{
    protected override string DirectoryName  => "DownloadedPosts";
    protected override string FileName => "DownloadedPosts";

    public DownloadedPosts LoadPosts()
    {
        var data = Load()
                   ?? new DownloadedPosts(new List<PostEntry>());

        Save(data);

        return data;
    }
}