using CommandLine;

namespace Toolkit.Commands;

[Verb("check-translations", HelpText = "Manage the language to update those")]
public class CheckTranslationsCommand
{
    [Option('i', "input", Required = true, HelpText = "Input paths from where you want to parse the files")]
    public string InputPath { get; set; }

    [Option('o', "output", HelpText = "The output you wants to generate your parser")]
    public string OutputPath { get; set; }
}