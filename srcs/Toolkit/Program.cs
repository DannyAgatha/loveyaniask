// NosEmu
// 


using System.Text;
using System.Threading.Tasks;
using CommandLine;
using Plugin.Database.Mapping;
using Toolkit.CommandHandlers;
using Toolkit.Commands;

namespace Toolkit;

public class Program
{
    #region Methods

    public static async Task<int> Main(string[] args)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        NonGameMappingRules.InitializeMapping();
        return await Parser.Default
            .ParseArguments<CheckTranslationsCommand, CreateAccountCommand, GenerateTranslationsCommand>(args)
            .MapResult(
                async (GenerateTranslationsCommand command) => await GenerateTranslationsCommandHandler.HandleAsync(command),
                async (CreateAccountCommand command) => await CreateAccountCommandHandler.HandleAsync(command),
                async (CheckTranslationsCommand command) => await CheckTranslationsCommandHandler.HandleAsync(command),
                errs => Task.FromResult(1)
            );
    }

    #endregion
}