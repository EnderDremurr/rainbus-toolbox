public interface IFileEditor
{
    string FilePath { get; }
    bool IsFileLoaded { get; }
    void LoadFile(string filePath);
}