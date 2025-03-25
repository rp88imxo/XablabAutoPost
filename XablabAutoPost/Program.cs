using VkNet;
using VkNet.Enums.Filters;
using VkNet.Model;
using XablabAutoPost.Core.Parser;
using XablabAutoPost.Core.PostCreator;
using XablabAutoPost.Framework.Application;
using XablabAutoPost.Framework.PostPublishers;
using XablabAutoPost.Framework.Promocode;
using XablabAutoPost.Framework.SettingsSaver;

namespace XablabAutoPost
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            ApplicationPersistentProvider applicationPersistentProvider = new ApplicationPersistentProvider();

            var applicationRunner = new ApplicationRunner(applicationPersistentProvider,
                new PostsFetcherFacade(applicationPersistentProvider),
                new List<IPostPublisher>()
                {
                    new VkPostPublisher(applicationPersistentProvider,
                        new VkDiscountProvider(applicationPersistentProvider))
                });

            await applicationRunner.RunAsync();
        }
    }
}