// NosEmu
// 


using System.Threading.Tasks;
using WingsEmu.Game.Networking;

namespace WingsEmu.Game.Commands;

public interface IGlobalCommandExecutor
{
    /// <summary>
    ///     Method which will parse the message and try to execute the command.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="sender"></param>
    void HandleCommand(string command, IClientSession sender, string prefix);

    /// <summary>
    ///     Method which will parse the message and try to execute the command.
    /// </summary>
    /// <param name="command">Raw message to parse.</param>
    /// <param name="sender"></param>
    Task HandleCommandAsync(string command, IClientSession sender, string prefix);
}