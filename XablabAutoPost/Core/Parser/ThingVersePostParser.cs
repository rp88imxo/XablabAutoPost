using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace XablabAutoPost.Core.Parser;

public readonly struct ParsePreviewsSettings
{
    public ParsePreviewsSettings(int page = 1)
    {
        Page = page;
    }

    public int Page { get; }
}

public readonly struct ParsePostSettings
{
    public ParsePostSettings()
    {
        
    }
    
}

public readonly struct ThingVersePostParserSettings
{
    public ThingVersePostParserSettings(string downloadDirectoryPath = "/Data/Downloaded/")
    {
        DownloadDirectoryPath = downloadDirectoryPath;
    }

    public string DownloadDirectoryPath { get; }
}

public class ThingVersePostParser : IDisposable
{
    private readonly ThingVersePostParserSettings _thingVersePostParserSettings;
    private readonly ChromeDriver _driver;

    public ThingVersePostParser(ThingVersePostParserSettings thingVersePostParserSettings)
    {
        _thingVersePostParserSettings = thingVersePostParserSettings;
        
        var options = new ChromeOptions();
        options.AddArgument("--headless");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--no-sandbox");
        
        options.AddUserProfilePreference("download.default_directory", _thingVersePostParserSettings.DownloadDirectoryPath);
        options.AddUserProfilePreference("intl.accept_languages", "nl");
        options.AddUserProfilePreference("disable-popup-blocking", "true");
        
        _driver = new ChromeDriver(options);
    }
    
    public async Task<IList<PostPreviewEntry>> ParsePreviews(ParsePreviewsSettings parsePreviewsSettings)
    {
        string url = $"https://www.thingiverse.com/?page={parsePreviewsSettings.Page}";
        
        await _driver.Navigate().GoToUrlAsync(url);
        await Task.Delay(1000);
            
        AcceptCookies();
        
        var postElements = _driver.FindElements(By.CssSelector(".ItemCardContainer__itemCard--GGbYM"));

        var postPreviews = new List<PostPreviewEntry>();
            
        foreach (var postElement in postElements)
        {
            try
            {
                var link = postElement.FindElement(By.CssSelector(".ItemCardHeader__itemCardHeader--cPULo"));

                var href = link.GetAttribute("href");

                var postNameElement =
                    postElement.FindElement(By.CssSelector(".ItemCardHeader__itemCardHeader--cPULo"));

                var postName = postNameElement.Text.Trim();

                if (postName == "Advertisement")
                {
                    continue;
                }
                   
                //var imageElement = postElement.FindElement(By.CssSelector(".thing-img img"));
                //var descElement = postElement.FindElement(By.CssSelector(".thing-name"));
                   
                //string imageUrl = imageElement.GetAttribute("src");
                //string description = descElement.Text.Trim();

                var id = ParseId(href);

                if (string.IsNullOrEmpty(id))
                {
                    continue;
                }
                
                postPreviews.Add(new PostPreviewEntry
                {
                    PostName = postName,
                    PostUri = href,
                    Id = id
                });
            }
            catch (NoSuchElementException)
            {
                
            }
        }
            
        return postPreviews;
    }

    private void AcceptCookies()
    {
        try
        {
            var buttonAllowAll = _driver.FindElement(By.Id("CybotCookiebotDialogBodyLevelButtonLevelOptinAllowAll"));
            buttonAllowAll.Click();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public async Task<IList<PostEntryRaw>> ParsePosts(IList<PostPreviewEntry> postPreviewEntries, ParsePostSettings parsePostSettings)
    {
        var postEntries = new List<PostEntryRaw>();

        foreach (var postPreview in postPreviewEntries)
        {
            try
            {
                await _driver.Navigate().GoToUrlAsync(postPreview.PostUri);
                await Task.Delay(3000);
            
                var slideElementOfImage = _driver.FindElement(By.CssSelector(".Carousel__carouselContent--J16cg"));
                var imageElement = slideElementOfImage.FindElement(By.CssSelector(".mediaItem--image"));

                var srcOfImage = imageElement.GetAttribute("src");

               var (fileName, filePath) = DownloadFile(postPreview);
               
               postEntries.Add(new PostEntryRaw
               {
                   PostId = postPreview.Id,
                   MainImageSource = srcOfImage,
                   FileName = fileName,
                   FilePath = filePath,
                   PostName = postPreview.PostName
               });
               
               await Task.Delay(500);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        return postEntries;
    }

    private string? ParseId(string uri)
    {
        if (string.IsNullOrEmpty(uri))
        {
            return null;
        }

        return uri.Substring(uri.LastIndexOf(':') + 1);
    }

    private (string, string) DownloadFile(PostPreviewEntry postPreview)
    {
        var fileName = $"{postPreview.PostName} - {postPreview.Id}.zip";
        var filePath = $"{_thingVersePostParserSettings.DownloadDirectoryPath}/{fileName}";

        if (File.Exists(filePath))
        {
            return (fileName, filePath);
        }
        
        var downloadButton = _driver.FindElement(By.CssSelector(
            "button[class = 'tv-button tv-button--primary tv-button--large tv-button--rounded tv-button--icon-right']"));
        downloadButton.Click();
               
        WebDriverWait waitDownload = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
        waitDownload.Until<bool>(x=> File.Exists(filePath));

        return (fileName, filePath);
    }

    public void Dispose()
    {
        _driver.Dispose();
    }
}

public class PostEntryRaw
{
    public string PostId { get; set; }
    public string PostName { get; set; }
    public string MainImageSource { get; set; }
    public string FileName { get; set; }
    public string FilePath{ get; set; }
}

public class PostPreviewEntry
{
    public string PostName { get; set; }
    public string PostUri { get; set; }
    public string Id { get; set; }
}