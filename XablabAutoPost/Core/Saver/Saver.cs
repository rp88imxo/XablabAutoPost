using System.Diagnostics;
using Newtonsoft.Json;

namespace XablabAutoPost.Core.Saver;

public abstract class Saver<T>
{
    private const int DEFAULT_SLOT = 0;
    protected abstract string DirectoryName { get; }
    protected abstract string FileName { get; }

    public void Save(T data, int slot = DEFAULT_SLOT)
    {
        string filepath = GetFilepath(slot);
        string serializedData = JsonConvert.SerializeObject(data);

        var file = new FileInfo(filepath);
        file.Directory?.Create();

        File.WriteAllText(file.FullName, serializedData);
    }

    public T? Load(int slot = DEFAULT_SLOT)
    {
        if (!IsSaveExists(slot)) return default;

        string filepath = GetFilepath(slot);
        string data = File.ReadAllText(filepath);

        try
        {
            var deserializedData = JsonConvert.DeserializeObject<T>(data);
            return deserializedData;
        }
        catch (Exception e)
        {
            Console.WriteLine(
                $"Unable to load {FileName} at slot {slot}. Resetting...");
            Console.WriteLine(e);
            Reset();
        }

        return default;
    }

    public bool IsSaveExists(int slot = DEFAULT_SLOT)
    {
        return File.Exists(GetFilepath(slot));
    }

    public void Reset()
    {
        var mainSavePath = GetMainSavePath();

        if (!Directory.Exists(mainSavePath))
        {
            return;
        }

        Directory.Delete(mainSavePath, true);
    }

    private string GetMainSavePath()
    {
        var separator = Path.DirectorySeparatorChar;
        return
            $"Data{separator}Settings{separator}{DirectoryName}{separator}";
    }

    private string GetFilepath(int slot)
    {
        var separator = Path.DirectorySeparatorChar;
        return
            $"Data{separator}Settings{separator}{DirectoryName}{separator}{separator}{FileName}";
    }
}