using System.Diagnostics;
using System.Timers;
using XablabAutoPost.Core.ConsoleLogger;
using XablabAutoPost.Framework.PostPublishers;
using XablabAutoPost.Framework.SettingsSaver;
using Timer = System.Timers.Timer;

namespace XablabAutoPost.Framework.Application;

public class ApplicationRunner
{
    private const int DelayBetweenUpdates = 3000;
    
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
        ConsoleLogger.Log("ApplicationRunner", "Initializing auto poster...", ConsoleColor.Green);
        
        _applicationPersistentProvider = applicationPersistentProvider;
        _postsFetcherFacade = postsFetcherFacade;
        _postPublishers = postPublishers;

        _settings = _applicationPersistentProvider.SettingsSaver.LoadSettings();

        _stopwatch = new Stopwatch();

        _firstLaunch = true;
        
        ConsoleLogger.Log("ApplicationRunner", "Initialized successfully...", ConsoleColor.Green);
    }

    public void Stop()
    {
        _cancellationTokenSource.Cancel();
    }
    
    public async Task RunAsync()
    {
        ConsoleLogger.Log("ApplicationRunner", "Starting auto poster...", ConsoleColor.Gray);
        
        _cancellationTokenSource = new CancellationTokenSource();
        _cancelationToken = _cancellationTokenSource.Token;
        
        _stopwatch.Start();

        var nextPostTime = TimeSpan.Zero;
        if (_settings.SavedPassedTime != TimeSpan.Zero)
        {
            if (_settings.SavedPassedTime < _settings.PeriodOfPost)
            {
                nextPostTime = _settings.PeriodOfPost - _settings.SavedPassedTime;
            }
        }
        else
        {
            nextPostTime = _settings.PeriodOfPost;
        }
        
        while (true)
        {
            if (_cancelationToken.IsCancellationRequested)
            {
                return;
            }
            
            if (Console.KeyAvailable && Console.ReadKey(false).Key == ConsoleKey.Escape)
            {
                Console.WriteLine("Requested Exit");
                Stop();
                continue;
            }
            
            if ( _stopwatch.Elapsed > nextPostTime || _firstLaunch)
            {
                _firstLaunch = false;
                await MakePost();
                
                _stopwatch.Restart();
                
                _settings.SavedPassedTime = _stopwatch.Elapsed;
                _applicationPersistentProvider.SettingsSaver.Save(_settings);
                
                nextPostTime = _settings.PeriodOfPost;
            }
            else
            {
                _settings.SavedPassedTime = _stopwatch.Elapsed;
                _applicationPersistentProvider.SettingsSaver.Save(_settings);
            }
            
            await Task.Delay(DelayBetweenUpdates, _cancelationToken);
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