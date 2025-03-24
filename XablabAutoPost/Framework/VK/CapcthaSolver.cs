using VkNet.Utils.AntiCaptcha;

namespace XablabAutoPost.Framework.VK;

public class CapcthaSolver : ICaptchaSolver
{
    public string Solve(string url)
    {
        Console.Write($"Введите капчу {url}: ");
        var capcha = Console.ReadLine(); 
        return capcha;
    }

    public void CaptchaIsFalse()
    {
        
    }
}