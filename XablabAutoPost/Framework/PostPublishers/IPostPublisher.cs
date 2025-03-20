using XablabAutoPost.Core.PostCreator;

namespace XablabAutoPost.Framework.PostPublishers;

public interface IPostPublisher
{
    public Task PublishPostsAsync(IList<PostEntry> postEntries);
    public Task PublishPostAsync(PostEntry postEntry);
}