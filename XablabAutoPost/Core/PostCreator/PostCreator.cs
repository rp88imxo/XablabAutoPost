using XablabAutoPost.Core.Parser;
using XablabAutoPost.Core.ImageDownloader;

namespace XablabAutoPost.Core.PostCreator;

public readonly struct PostCreatorSettings
{
    public PostCreatorSettings(string postsSavePath)
    {
        PostsSavePath = postsSavePath;
    }

    public string PostsSavePath { get; } = "D:\\MyStuff\\C#\\XablabAutoPost\\XablabAutoPost\\Data\\Posts";
}

[Serializable]
public class PostEntry
{
    public string PostId { get; set; }
    public string PostName { get; set; }
    public string MainImagePath { get; set; }
    public string FileName { get; set; }
    public string FilePath{ get; set; }
    public string PostUri { get; set; }
}

public class PostCreator
{
    private readonly PostCreatorSettings _postCreatorSettings;
    private readonly ImageDownloader.ImageDownloader _imageDownloader;

    public PostCreator(PostCreatorSettings postCreatorSettings)
    {
        _postCreatorSettings = postCreatorSettings;
        _imageDownloader = new ImageDownloader.ImageDownloader();
    }
    
    public async Task<IList<PostEntry>> CreatePostsFromRawAsync(IList<PostEntryRaw> postEntries)
    {
        var postsReady = new List<PostEntry>(postEntries.Count);
        
        foreach (var postEntry in postEntries)
        {
            var postDirectory = $"{_postCreatorSettings.PostsSavePath}\\{postEntry.PostId}";
            
            string? imagePath = null;
            
            if (Directory.Exists(postDirectory))
            {
                var files =  Directory.GetFiles(postDirectory);

                if (files.Length == 0)
                {
                    continue;
                }
                
                imagePath = files[0];
                
                postsReady.Add(new PostEntry
                {
                    PostId = postEntry.PostId,
                    PostName = postEntry.PostName,
                    MainImagePath = imagePath,
                    FileName = postEntry.FileName,
                    FilePath = postEntry.FilePath,
                    PostUri = postEntry.PostUri
                });
                
                continue;
            }
            
            Directory.CreateDirectory(postDirectory);

            var loadingContext = await _imageDownloader.DownloadImageAsync(new Uri(postEntry.MainImageSource));
            
            if (loadingContext.ImageBytes.Length != 0)
            {
                var uriWithoutQuery = loadingContext.Uri.GetLeftPart(UriPartial.Path);
                var fileExtension = Path.GetExtension(uriWithoutQuery);
                imagePath = Path.Combine(postDirectory, $"mainImage{fileExtension}");

                await File.WriteAllBytesAsync(imagePath, loadingContext.ImageBytes);
            }
            
            postsReady.Add(new PostEntry
            {
                PostId = postEntry.PostId,
                PostName = postEntry.PostName,
                MainImagePath = imagePath,
                FileName = postEntry.FileName,
                FilePath = postEntry.FilePath,
                PostUri = postEntry.PostUri
            });
        }

        return postsReady;
    }
}