using CommandLine;

namespace Toolkit.Commands;

[Verb("translations", HelpText = "Manage the language to update those")]
public class GenerateTranslationsCommand
{
    [Option('l', "language", Default = "all", HelpText = "Language that will be used")]
    public string Language { get; set; }

    [Option('i', "input", Required = true, HelpText = "Input paths from where you want to parse the files")]
    public string InputPath { get; set; }

    [Option('o', "output", HelpText = "The output you wants to generate your parser")]
    public string OutputPath { get; set; }
}