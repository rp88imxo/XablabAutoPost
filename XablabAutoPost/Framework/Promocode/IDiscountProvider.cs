namespace XablabAutoPost.Framework.Promocode;

public class DiscountEntry
{
    public DiscountEntry(string code, int value)
    {
        Code = code;
        Value = value;
    }

    public string Code { get; set; }
    public int Value { get; set; }
}

public interface IDiscountProvider
{
    DiscountEntry GetDiscount();
    void GenerateNewDiscount(DateTime currentTimeUtc);
}