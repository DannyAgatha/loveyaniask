using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WingsAPI.Communication;
using WingsAPI.Communication.Translations;
using WingsEmu.Game.Managers;

namespace NosEmu.Plugins.BasicImplementations.ForbiddenNames;

public class ReloadableForbiddenNamesManager : IForbiddenNamesManager
{
    private readonly ITranslationService _translationService;
    private List<string> _bannedNames = new();

    public ReloadableForbiddenNamesManager(ITranslationService translationService) => _translationService = translationService;

    public bool IsBanned(string name, out string s)
    {
        string lowerCharName = name.ToLowerInvariant();

        // shallow copy reference
        List<string> scopedBannedNames = _bannedNames;
        foreach (string bannedName in scopedBannedNames)
        {
            if (!lowerCharName.Contains(bannedName))
            {
                continue;
            }

            s = bannedName;
            return true;
        }

        s = string.Empty;
        return false;
    }

    public async Task Reload()
    {
        GetForbiddenWordsResponse response = await _translationService.GetForbiddenWords(new EmptyRpcRequest());
        Interlocked.Exchange(ref _bannedNames, response.ForbiddenWords?.ToList() ?? new List<string>());
    }
}