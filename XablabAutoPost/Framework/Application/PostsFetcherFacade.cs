using XablabAutoPost.Core.ConsoleLogger;
using XablabAutoPost.Core.Parser;
using XablabAutoPost.Core.PostCreator;
using XablabAutoPost.Framework.SettingsSaver;
using XablabAutoPost.Framework.Utils;

namespace XablabAutoPost.Framework.Application;

public class PostsFetcherFacade
{
    private const string WatermarkText = "Xablab.ru";
    
    private readonly ApplicationPersistentProvider _applicationPersistentProvider;
    private readonly ThingVersePostParser _thingVersePostParser;
    private readonly PostCreator _postCreator;
    private ApplicationSettings _settings;
    private DownloadedPosts _loadedPosts;
    private UsedPostsData _usedPosts;
    private readonly WatermarkProcessor _watermarkProcessor;

    public PostsFetcherFacade(ApplicationPersistentProvider applicationPersistentProvider)
    {
        ConsoleLogger.Log("PostsFetcherFacade", "Initializing PostsFetcherFacade...",
            ConsoleColor.Green);
        
        _applicationPersistentProvider = applicationPersistentProvider;
        
         _settings = _applicationPersistentProvider.SettingsSaver.LoadSettings();
         _loadedPosts = _applicationPersistentProvider.DownloadedPostsSaver.LoadPosts();
         _usedPosts = _applicationPersistentProvider.UsedPostsSaver.LoadUsedPosts();
        
        var thingVersePostParserSettings =
            new ThingVersePostParserSettings(_settings.DownloadDirectoryPath);

        _thingVersePostParser = new ThingVersePostParser(thingVersePostParserSettings);

        _postCreator =
            new PostCreator(
                new PostCreatorSettings(_settings.PostsSavePath));

        _watermarkProcessor = new WatermarkProcessor();
        
        ConsoleLogger.Log("PostsFetcherFacade", "Initialized PostsFetcherFacade successfully!",
            ConsoleColor.Green);
    }

    public async Task<IList<PostEntry>> RequestPostsAsync()
    {
        _settings = _applicationPersistentProvider.SettingsSaver.LoadSettings();
        _loadedPosts = _applicationPersistentProvider.DownloadedPostsSaver.LoadPosts();
        _usedPosts = _applicationPersistentProvider.UsedPostsSaver.LoadUsedPosts();
        
        var notUsedPosts = new List<PostPreviewEntry>();

        int currentPage = 1;
        while (notUsedPosts.Count < _settings.PostsToFetch)
        {
            var previews = await _thingVersePostParser.ParsePreviews(new ParsePreviewsSettings(currentPage));
            
            foreach (var preview in previews)
            {
                if (!_usedPosts.UsedPostsIds.Contains(preview.Id))
                {
                    notUsedPosts.Add(preview);
                }

                if (notUsedPosts.Count >= _settings.PostsToFetch)
                {
                    break;
                }
            }
            
            currentPage++;
        }

        var postsToLoad = new List<PostPreviewEntry>();

        var loadedPostsHashSet = _loadedPosts.PostEntries.Select(x => x.PostId).ToHashSet();

        foreach (var notUsedPost in notUsedPosts)
        {
            if (!loadedPostsHashSet.Contains(notUsedPost.Id))
            {
                postsToLoad.Add(notUsedPost);
            }
        }

        var postEntries =
            await _thingVersePostParser.ParsePosts(postsToLoad,
                new ParsePostSettings());

        var resultPosts = await _postCreator.CreatePostsFromRawAsync(postEntries);

        foreach (var resultPost in resultPosts)
        {
            if (!string.IsNullOrEmpty(resultPost.MainImagePath) )
            {
              var addedWatermark =  _watermarkProcessor.TryAddWatermark(resultPost.MainImagePath, WatermarkText);
              resultPost.HasWatermark = addedWatermark;
            }
        }
        
        foreach (var resultPost in resultPosts)
        {
            _loadedPosts.PostEntries.Add(resultPost);
        }

        _applicationPersistentProvider.DownloadedPostsSaver.Save(_loadedPosts);

        var newPosts = new List<PostEntry>();
        var notUsedPostsHashSet = notUsedPosts.Select(x => x.Id).ToHashSet();
        foreach (var loadedPostsPostEntry in _loadedPosts.PostEntries)
        {
            if (notUsedPostsHashSet.Contains(loadedPostsPostEntry.PostId))
            {
                newPosts.Add(loadedPostsPostEntry);
            }
        }

        return newPosts;
    }
}