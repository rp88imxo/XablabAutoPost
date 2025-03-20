using Newtonsoft.Json;
using XablabAutoPost.Core.Saver;

namespace XablabAutoPost.Framework.SettingsSaver;

public class ApplicationSettings
{
    public ApplicationSettings(TimeSpan periodOfPost, string downloadDirectoryPath, string postsSavePath,
        int postsToFetch, int postsToPublish)
    {
        PeriodOfPost = periodOfPost;
        DownloadDirectoryPath = downloadDirectoryPath;
        PostsSavePath = postsSavePath;
        PostsToFetch = postsToFetch;
        PostsToPublish = postsToPublish;
    }

    [JsonProperty("period_of_post")] public TimeSpan PeriodOfPost { get; set; }


    [JsonProperty("download_directory_path")]
    public string DownloadDirectoryPath { get; set; }

    [JsonProperty("posts_save_path")] public string PostsSavePath { get; set; }

    [JsonProperty("posts_to_fetch")] public int PostsToFetch { get; set; }

    [JsonProperty("posts_to_publish")] public int PostsToPublish { get; set; }
}

public class SettingsSaver : Saver<ApplicationSettings>
{
    protected override string DirectoryName => "Application";
    protected override string FileName => "ApplicationSettings";

    public ApplicationSettings LoadSettings()
    {
        var data = Load()
                   ?? new ApplicationSettings(TimeSpan.FromHours(1),
                       "D:\\MyStuff\\C#\\XablabAutoPost\\XablabAutoPost\\Data\\Downloaded",
                       "D:\\MyStuff\\C#\\XablabAutoPost\\XablabAutoPost\\Data\\Posts", 5, 1);

        Save(data);

        return data;
    }
}