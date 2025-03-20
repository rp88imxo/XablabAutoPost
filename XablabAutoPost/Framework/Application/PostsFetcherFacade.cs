using XablabAutoPost.Core.Parser;
using XablabAutoPost.Core.PostCreator;
using XablabAutoPost.Framework.SettingsSaver;

namespace XablabAutoPost.Framework.Application;

public class PostsFetcherFacade
{
    private readonly ApplicationPersistentProvider _applicationPersistentProvider;

    public PostsFetcherFacade(ApplicationPersistentProvider applicationPersistentProvider)
    {
        _applicationPersistentProvider = applicationPersistentProvider;
    }

    public async Task<IList<PostEntry>> RequestPostsAsync()
    {
        var settings = _applicationPersistentProvider.SettingsSaver.LoadSettings();
        var loadedPosts = _applicationPersistentProvider.DownloadedPostsSaver.LoadPosts();
        var usedPosts = _applicationPersistentProvider.UsedPostsSaver.LoadUsedPosts();

        var thingVersePostParserSettings =
            new ThingVersePostParserSettings(settings.DownloadDirectoryPath);

        var thingVersePostParser = new ThingVersePostParser(thingVersePostParserSettings);

        var postCreator =
            new PostCreator(
                new PostCreatorSettings(settings.PostsSavePath));

        var notUsedPosts = new List<PostPreviewEntry>();

        int currentPage = 1;
        while (notUsedPosts.Count < settings.PostsToFetch)
        {
            var previews = await thingVersePostParser.ParsePreviews(new ParsePreviewsSettings(currentPage));
            
            foreach (var preview in previews)
            {
                if (!usedPosts.UsedPostsIds.Contains(preview.Id))
                {
                    notUsedPosts.Add(preview);
                }

                if (notUsedPosts.Count >= settings.PostsToFetch)
                {
                    break;
                }
            }
            
            currentPage++;
        }

        var postsToLoad = new List<PostPreviewEntry>();

        var loadedPostsHashSet = loadedPosts.PostEntries.Select(x => x.PostId).ToHashSet();

        foreach (var notUsedPost in notUsedPosts)
        {
            if (!loadedPostsHashSet.Contains(notUsedPost.Id))
            {
                postsToLoad.Add(notUsedPost);
            }
        }

        var postEntries =
            await thingVersePostParser.ParsePosts(postsToLoad,
                new ParsePostSettings());

        var resultPosts = await postCreator.CreatePostsFromRawAsync(postEntries);

        foreach (var resultPost in resultPosts)
        {
            loadedPosts.PostEntries.Add(resultPost);
        }

        _applicationPersistentProvider.DownloadedPostsSaver.Save(loadedPosts);

        var newPosts = new List<PostEntry>();
        var notUsedPostsHashSet = notUsedPosts.Select(x => x.Id).ToHashSet();
        foreach (var loadedPostsPostEntry in loadedPosts.PostEntries)
        {
            if (notUsedPostsHashSet.Contains(loadedPostsPostEntry.PostId))
            {
                newPosts.Add(loadedPostsPostEntry);
            }
        }

        return newPosts;
    }
}