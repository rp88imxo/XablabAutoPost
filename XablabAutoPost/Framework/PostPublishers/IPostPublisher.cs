using XablabAutoPost.Core.PostCreator;

namespace XablabAutoPost.Framework.PostPublishers;

public enum PublishResult
{
    Success,
    Fail
}

public interface IPostPublisher
{
    public Task PublishPostsAsync(IList<PostEntry> postEntries);
    public Task<PublishResult> PublishPostAsync(PostEntry postEntry);
}