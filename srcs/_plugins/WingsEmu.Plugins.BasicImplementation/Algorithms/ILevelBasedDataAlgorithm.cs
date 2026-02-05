namespace NosEmu.Plugins.BasicImplementations.Algorithms;

public interface ILevelBasedDataAlgorithm
{
    long[] Data { get; set; }
    void Initialize();
}