using CommandLine;

namespace Toolkit.Commands;

[Verb("create-accounts", HelpText = "Manage the language to update those")]
public class CreateAccountCommand
{
    [Option('e', "env", Required = false, HelpText = "Input paths from where you want to parse the files", Default = "toolkit.env")]
    public string EnvFile { get; set; }
}