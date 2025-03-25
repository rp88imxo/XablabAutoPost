namespace XablabAutoPost.Framework.SettingsSaver;

public class ApplicationPersistentProvider
{
    public SettingsSaver SettingsSaver { get; }
    public DownloadedPostsSaver DownloadedPostsSaver { get; }

    public UsedPostsSaver UsedPostsSaver { get; }

    public VkPublishSettingsSaver VkPublishSettingsSaver { get; }
    public VkMessagesTemplatesSaver VkMessagesTemplatesSaver { get; }
    public VkDiscountSaver VkDiscountSaver { get; }

    public ApplicationPersistentProvider()
    {
        SettingsSaver = new SettingsSaver();
        DownloadedPostsSaver = new DownloadedPostsSaver();
        UsedPostsSaver = new UsedPostsSaver();
        VkPublishSettingsSaver = new VkPublishSettingsSaver();
        VkMessagesTemplatesSaver = new VkMessagesTemplatesSaver();
        VkDiscountSaver = new VkDiscountSaver();
    }
}