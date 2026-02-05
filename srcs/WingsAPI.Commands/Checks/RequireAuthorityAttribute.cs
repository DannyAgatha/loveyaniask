// NosEmu
// 


using System.Threading.Tasks;
using Qmmands;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;

namespace WingsEmu.Commands.Checks
{
    public sealed class RequireAuthorityAttribute : CheckAttribute
    {
        public RequireAuthorityAttribute(AuthorityType authority) => Authority = authority;

        /// <summary>
        ///     This represents the Authority level required to execute a command.
        /// </summary>
        public AuthorityType Authority { get; }

        /// <inheritdoc />
        /// <summary>
        ///     This is a check (pre-condition) before trying to execute a command that needs to pass this check.
        /// </summary>
        /// <param name="context">Context of the command. It needs to be castable to a WingsEmuIngameCommandContext in our case.</param>
        /// <returns></returns>
        public override ValueTask<CheckResult> CheckAsync(CommandContext context)
        {
            if (context is not WingsEmuIngameCommandContext ctx)
            {
                return new ValueTask<CheckResult>(new CheckResult("Invalid context. This is *very* bad. Please report this."));
            }

            if (ctx.Player?.Account is not null && ctx.Player.Account.Authority < Authority)
            {
                return new ValueTask<CheckResult>(new CheckResult("You (at least) need to be a " + Authority + " in order to execute that command."));
            }

            return new ValueTask<CheckResult>(CheckResult.Successful);
        }
    }
}