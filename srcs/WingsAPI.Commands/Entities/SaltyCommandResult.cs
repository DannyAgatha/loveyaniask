// NosEmu
// 


using Qmmands;

namespace WingsEmu.Commands.Entities
{
    public sealed class SaltyCommandResult : CommandResult
    {
        public SaltyCommandResult(bool isSuccessful, string message = null)
        {
            IsSuccessful = isSuccessful;
            Message = message;
        }

        public override bool IsSuccessful { get; }
        public string Message { get; }
    }
}