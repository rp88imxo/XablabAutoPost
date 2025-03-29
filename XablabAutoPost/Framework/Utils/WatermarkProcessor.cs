using SixLabors.Fonts;
using SixLabors.ImageSharp.Drawing.Processing;
using XablabAutoPost.Core.ConsoleLogger;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;

namespace XablabAutoPost.Framework.Utils;

public class WatermarkProcessor
{
    public bool TryAddWatermark(string imagePath, string text)
    {
        try
        {
            // Загружаем шрифт. Убедитесь, что шрифт доступен системе,
            // или укажите путь к файлу шрифта .ttf/.otf
            // Для кроссплатформенности лучше добавить файл шрифта в проект и указать путь к нему.
            // Пример с системным шрифтом (может не работать на всех ОС):
            var fontFamily = SystemFonts.Collection.TryGet("Arial", out var family)
                ? family
                : SystemFonts.Collection.Families.FirstOrDefault();
            // Или загрузка из файла (поместите Roboto-Regular.ttf в папку проекта, установите "Copy to Output Directory"):
            // FontCollection collection = new ();
            // FontFamily family = collection.Add("Roboto-Regular.ttf"); // Путь к файлу шрифта

            using (var image = Image.Load(imagePath)) // Загружаем изображение
            {
                // Определяем размер шрифта (можно сделать зависимым от размера изображения)
                int fontSize = Math.Max(24, image.Height / 30); // Пример: размер шрифта зависит от высоты изображения
                var font = fontFamily.CreateFont(fontSize, FontStyle.Bold);

                // Настройки текста
                var options = new RichTextOptions(font)
                {
                    Origin = new PointF(image.Width - 10, image.Height - 10), // Позиция (правый нижний угол с отступом)
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    WrappingLength = image.Width * 0.8f // Макс. ширина текста
                };

                // Цвет и прозрачность водяного знака (белый, полупрозрачный)
                var color = Color.Gray.WithAlpha(0.45f); // 45% прозрачности

                // Рисуем текст
                image.Mutate(ctx => ctx.DrawText(options, text, color));

                // Сохраняем измененное изображение поверх старого
                image.Save(imagePath);

                ConsoleLogger.Log("PostCreator", $"Watermark added to: {imagePath}", ConsoleColor.Cyan);
                return true;
            }
        }
        catch (Exception ex)
        {
            // Логируем ошибку, но не прерываем основной процесс
            ConsoleLogger.Log("WatermarkProcessor", $"Failed to add watermark to {imagePath}: {ex.Message}", ConsoleColor.Red);
            return false;
        }
    }
}