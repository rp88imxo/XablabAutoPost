using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using XablabAutoPost.Core.ConsoleLogger;

namespace XablabAutoPost.Framework.Utils;

public class ImageFormatProcessor
{
    public ImageFormatProcessor()
    {
        Configuration.Default.ImageFormatsManager.SetEncoder(JpegFormat.Instance, new JpegEncoder()
        {
            Quality = 90
        });
    }
    
    public string? Process(string path)
    {
        try
        {
            if (Path.GetExtension(path).Equals(".jpg", StringComparison.OrdinalIgnoreCase)  
                || Path.GetExtension(path).Equals(".png", StringComparison.OrdinalIgnoreCase)) 
            {
                return path;
            }
            
            using (var image = Image.Load(path))
            {
                image.SaveAsJpeg(path);
                
                return path;
            }
        }
        catch (Exception ex)
        {
            // Логируем ошибку, но не прерываем основной процесс
            ConsoleLogger.Log("ImageFormatProccessor", $"Failed to change format of image at path {path}: {ex.Message}", ConsoleColor.Red);
            return null;
        }
    }
}