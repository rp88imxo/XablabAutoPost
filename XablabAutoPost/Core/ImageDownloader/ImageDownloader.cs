namespace XablabAutoPost.Core.ImageDownloader;


public class ImageDownloader
{
    private readonly HttpClient _httpClient;

    public ImageDownloader()
    {
        _httpClient = new HttpClient();
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }

    public async Task<LoadingContext> DownloadImageAsync(Uri uri)
    {
        try
        {
            var imageBytes = await _httpClient.GetByteArrayAsync(uri);
            return new LoadingContext(imageBytes, uri);
        }
        catch (Exception ex)
        {
            return new LoadingContext(new byte[0], uri);
        }
    }
}

public class LoadingContext
{
    public byte[] ImageBytes;
    public Uri Uri;

    public LoadingContext(byte[] imageBytes, Uri uri)
    {
        ImageBytes = imageBytes;
        Uri = uri;
    }
}