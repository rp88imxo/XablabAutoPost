using XablabAutoPost.Core.Saver;

namespace XablabAutoPost.Framework.SettingsSaver;

[Serializable]
public class VkDiscountData
{
    public VkDiscountData(string currentDiscount, DateTime discountExpirationTime, TimeSpan discountSpan, int discountValue)
    {
        CurrentDiscount = currentDiscount;
        DiscountExpirationTime = discountExpirationTime;
        DiscountSpan = discountSpan;
        DiscountValue = discountValue;
    }
    
    public DateTime DiscountExpirationTime { get; set; }
    public string CurrentDiscount { get; set; }
    
    public int DiscountValue { get; set; }
    
    public TimeSpan DiscountSpan { get; set; }
}

public class VkDiscountSaver : Saver<VkDiscountData>
{
    protected override string DirectoryName => "VkDiscountData";
    protected override string FileName  => "VkDiscountData";

    public VkDiscountData LoadDiscountData()
    {
        var data = Load()
                   ?? new VkDiscountData(string.Empty, DateTime.MinValue, TimeSpan.FromHours(2), 0);

        Save(data);

        return data;
    }
}