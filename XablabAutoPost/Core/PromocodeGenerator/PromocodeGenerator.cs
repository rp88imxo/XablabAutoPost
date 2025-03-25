namespace XablabAutoPost.Core.PromocodeGenerator;

public class PromocodeGenerator
{
    public static string GeneratePromoCode(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
    
    public static int GetRandomDiscount()
    {
        int[] discounts = { 5, 10, 15, 20 };
        int[] weights = { 40, 25, 20, 15 };
        
        var randomValue = Random.Shared.Next(100);
        var cumulative = 0;
        
        for (var i = 0; i < discounts.Length; i++)
        {
            cumulative += weights[i];
            if (randomValue <= cumulative)
            {
                return discounts[i];
            }
        }
        
        return discounts[0];
    }
}