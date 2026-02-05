namespace NosEmu.Plugins.BasicImplementations.ServerConfigs.ImportObjects.Files;

public interface IExportableFile<out T> where T : IFileData
{
    string FileName { get; }
    T Data { get; }
}