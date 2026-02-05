using WingsEmu.Game._i18n;
using WingsEmu.Game.Configurations.Miniland;
using WingsEmu.Game.Miniland;
using WingsEmu.Game.Networking;

namespace WingsAPI.Game.Extensions.MinilandExtensions
{
    public static class MinilandPacketExtensions
    {
        public static string GenerateMinilandPrivateInformation(this IClientSession session, IMinilandManager minilandManager, IGameLanguageService languageService)
        {
            int visitCount = minilandManager.GetMinilandVisitCounter(session.PlayerEntity.Id).ConfigureAwait(false).GetAwaiter().GetResult();
            Miniland miniland = minilandManager.GetMinilandConfiguration(session.PlayerEntity.Miniland);
            return $"mlinfo {miniland.MapItemVnum.ToString()} {session.PlayerEntity.MinilandPoint.ToString()} 100 {visitCount}" +
                $" {session.PlayerEntity.LifetimeStats.TotalMinilandVisits} {minilandManager.GetMinilandMaximumCapacity(session.PlayerEntity.Id).ToString()} {((byte)session.PlayerEntity.MinilandState).ToString()} {miniland.MapItemVnum.ToString()}" +
                $" {session.GetMinilandSerializedMessage(languageService)}";
        }

        public static string GenerateMinilandPublicInformation(this IClientSession session, IMinilandManager minilandManager, IGameLanguageService languageService)
        {
            IClientSession minilandOwner = minilandManager.GetSessionByMiniland(session.CurrentMapInstance);
            Miniland miniland = minilandManager.GetMinilandConfiguration(minilandOwner.PlayerEntity.Miniland);
            int visitCount = minilandManager.GetMinilandVisitCounter(minilandOwner.PlayerEntity.Id).ConfigureAwait(false).GetAwaiter().GetResult();
            return $"mlinfobr {miniland.MapItemVnum.ToString()} {minilandOwner.PlayerEntity.Name} {visitCount}" +
                $" {minilandOwner.PlayerEntity.LifetimeStats.TotalMinilandVisits} {minilandManager.GetMinilandMaximumCapacity(minilandOwner.PlayerEntity.Id).ToString()} {minilandOwner.GetMinilandSerializedMessage(languageService)}";
        }

        public static void SendMinilandPrivateInformation(this IClientSession session, IMinilandManager minilandManager, IGameLanguageService languageService) =>
            session.SendPacket(session.GenerateMinilandPrivateInformation(minilandManager, languageService));

        public static void SendMinilandPublicInformation(this IClientSession session, IMinilandManager minilandManager, IGameLanguageService languageService) =>
            session.SendPacket(session.GenerateMinilandPublicInformation(minilandManager, languageService));
    }
}