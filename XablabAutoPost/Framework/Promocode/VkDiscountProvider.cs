using XablabAutoPost.Core.PromocodeGenerator;
using XablabAutoPost.Framework.SettingsSaver;

namespace XablabAutoPost.Framework.Promocode;


public class VkDiscountProvider : IDiscountProvider
{
    private readonly ApplicationPersistentProvider _applicationPersistentProvider;
    private readonly VkDiscountData _discountData;

    public VkDiscountProvider(ApplicationPersistentProvider applicationPersistentProvider)
    {
        _applicationPersistentProvider = applicationPersistentProvider;
        _discountData =  _applicationPersistentProvider.VkDiscountSaver.LoadDiscountData();
    }
    
    public DiscountEntry GetDiscount()
    {
        var currentTime = DateTime.UtcNow;
        if (!string.IsNullOrEmpty(_discountData.CurrentDiscount ))
        {
            if (_discountData.DiscountExpirationTime != DateTime.MinValue && currentTime > _discountData.DiscountExpirationTime)
            {
                GenerateNewDiscount(currentTime);
            }
        }
        else
        {
            GenerateNewDiscount(currentTime);
        }
        
        return new DiscountEntry(_discountData.CurrentDiscount, _discountData.DiscountValue);
    }

    public void GenerateNewDiscount(DateTime currentTimeUtc)
    {
        _discountData.DiscountExpirationTime = currentTimeUtc + _discountData.DiscountSpan;
        _discountData.CurrentDiscount = $"VkXablab{PromocodeGenerator.GeneratePromoCode(4)}";
        _discountData.DiscountValue = PromocodeGenerator.GetRandomDiscount();
        SaveData();
    }

    private void SaveData()
    {
        _applicationPersistentProvider.VkDiscountSaver.Save(_discountData);
    }
}