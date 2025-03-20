using System.Diagnostics;
using System.Timers;
using XablabAutoPost.Framework.PostPublishers;
using XablabAutoPost.Framework.SettingsSaver;
using Timer = System.Timers.Timer;

namespace XablabAutoPost.Framework.Application;

public class ApplicationRunner
{
    private readonly ApplicationPersistentProvider _applicationPersistentProvider;
    private readonly PostsFetcherFacade _postsFetcherFacade;
    private readonly IList<IPostPublisher> _postPublishers;
    private readonly ApplicationSettings _settings;

    private Stopwatch _stopwatch;
    private CancellationTokenSource _cancellationTokenSource;
    private CancellationToken _cancelationToken;
    private bool _firstLaunch;

    public ApplicationRunner(
        ApplicationPersistentProvider applicationPersistentProvider, 
        PostsFetcherFacade postsFetcherFacade,
        IList<IPostPublisher> postPublishers)
    {
        _applicationPersistentProvider = applicationPersistentProvider;
        _postsFetcherFacade = postsFetcherFacade;
        _postPublishers = postPublishers;

        _settings = _applicationPersistentProvider.SettingsSaver.LoadSettings();

        _stopwatch = new Stopwatch();

        _firstLaunch = true;
    }

    public void Stop()
    {
        _cancellationTokenSource.Cancel();
    }
    
    public async Task RunAsync()
    {
        _cancellationTokenSource = new CancellationTokenSource();
        _cancelationToken = _cancellationTokenSource.Token;
        
        _stopwatch.Start();
        
        while (true)
        {
            if (_cancelationToken.IsCancellationRequested)
            {
                return;
            }
            
            if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
            {
                Console.WriteLine("Requested Exit");
                Stop();
                continue;
            }
            
            if ( _stopwatch.Elapsed > _settings.PeriodOfPost || _firstLaunch)
            {
                _firstLaunch = false;
                await MakePost();
            }
        }
    }
    
    private async Task MakePost()
    {
        var newPosts = await _postsFetcherFacade.RequestPostsAsync();

        if (newPosts.Count == 0)
        {
            return;
        }
        
        foreach (var postPublisher in _postPublishers)
        {
            await postPublisher.PublishPostsAsync(newPosts);
        }
    }
}