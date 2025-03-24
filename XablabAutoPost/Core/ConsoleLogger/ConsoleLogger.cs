namespace XablabAutoPost.Core.ConsoleLogger;

public class ConsoleLogger
{
    public static void Log(string logPrefix,string message, ConsoleColor consoleColor = ConsoleColor.White)
    {
        var oldColor = Console.ForegroundColor;
        Console.ForegroundColor = consoleColor;
        Console.WriteLine($"[{logPrefix}]: {message}");
        Console.ForegroundColor = oldColor;
    }
}