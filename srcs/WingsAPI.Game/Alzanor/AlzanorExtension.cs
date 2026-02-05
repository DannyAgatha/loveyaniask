using System;
using System.Collections.Generic;
using System.Text;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game.Alzanor.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;

namespace WingsEmu.Game.Alzanor;

public static class AlzanorExtension
{
    public static bool CanJoinToAlzanorEvent(this IClientSession session)
    {
        if (session.PlayerEntity.AlzanorComponent.IsInAlzanorEvent)
        {
            return false;
        }

        if (session.PlayerEntity.IsInGroup())
        {
            return false;
        }

        if (session.PlayerEntity.TimeSpaceComponent.IsInTimeSpaceParty)
        {
            return false;
        }

        if (session.PlayerEntity.IsInRaidParty)
        {
            return false;
        }

        if (session.PlayerEntity.HasShopOpened)
        {
            return false;
        }

        if (session.IsMuted())
        {
            return false;
        }

        return session.CurrentMapInstance != null && session.CurrentMapInstance.HasMapFlag(MapFlags.IS_BASE_MAP);
    }
}