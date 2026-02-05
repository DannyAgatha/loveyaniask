using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Mate;

public class PtCtlPacketHandler : GenericGamePacketHandlerBase<PtCtlPacket>
{
    protected override async Task HandlePacketAsync(IClientSession session, PtCtlPacket packet)
    {
        if (packet == null)
        {
            return;
        }

        if (session?.CurrentMapInstance == null)
        {
            return;
        }

        if (string.IsNullOrEmpty(packet.PacketEnd))
        {
            return;
        }

        string[] packetSplit = packet.PacketEnd.Split(' ');
        int expectedLength = packet.Amount * 3;

        if (packetSplit.Length < expectedLength)
        {
            return;
        }
        
        for (int i = 0; i < expectedLength; i += 3)
        {
            if (!int.TryParse(packetSplit[i], out int petId))
            {
                continue;
            }

            if (!short.TryParse(packetSplit[i + 1], out short positionX))
            {
                continue;
            }

            if (!short.TryParse(packetSplit[i + 2], out short positionY))
            {
                continue;
            }

            INpcEntity npc = session.PlayerEntity.MapInstance.GetNpcById(petId);
            if (npc != null)
            {
                CheckNpcMovement(session, npc, positionX, positionY);
                continue;
            }

            IMateEntity mateEntity = session.PlayerEntity.MateComponent.GetMate(s => s.Id == petId);
            if (mateEntity == null)
            {
                continue;
            }

            if (!mateEntity.IsAlive())
            {
                continue;
            }

            if (mateEntity.Owner.IsOnVehicle)
            {
                continue;
            }

            session.SendCondMate(mateEntity);

            if (!mateEntity.CanMove())
            {
                session.SendCondMate(mateEntity);
                continue;
            }

            if (session.CurrentMapInstance.IsBlockedZone(positionX, positionY))
            {
                continue;
            }

            if (mateEntity.MapInstance.IsBlockedZone(positionX, positionY))
            {
                continue;
            }

            int distance = mateEntity.Position.GetDistance(positionX, positionY);
            if (distance > 10)
            {
                continue;
            }

            if (mateEntity.MapInstance.HasMapFlag(MapFlags.IS_BASE_MAP))
            {
                mateEntity.MapX = positionX;
                mateEntity.MapY = positionY;
            }

            if (mateEntity.MapInstance.MapInstanceType == MapInstanceType.Miniland && mateEntity.MapInstance.Id == mateEntity.Owner.Miniland.Id)
            {
                mateEntity.MinilandX = positionX;
                mateEntity.MinilandY = positionY;
            }

            mateEntity.ChangePosition(new Position(positionX, positionY));
            session.BroadcastMovement(mateEntity, new ExceptSessionBroadcast(session));
        }
    }

    private void CheckNpcMovement(IClientSession session, INpcEntity npc, short positionX, short positionY)
    {
        if (npc.MapInstance.MapInstanceType != MapInstanceType.TimeSpaceInstance)
        {
            return;
        }

        if (!npc.IsTimeSpaceMate)
        {
            return;
        }

        List<INpcEntity> partners = session.PlayerEntity.TimeSpaceComponent.Partners;
        INpcEntity partner = partners.FirstOrDefault(x => x.MonsterVNum == npc.MonsterVNum);
        if (partner == null || !npc.CharacterPartnerId.HasValue || npc.Id != partner.Id)
        {
            return;
        }

        if (!npc.IsAlive())
        {
            return;
        }

        session.SendCondMate(npc);

        if (!npc.CanPerformMove())
        {
            session.SendCondMate(npc);
            return;
        }

        if (session.CurrentMapInstance.IsBlockedZone(positionX, positionY))
        {
            return;
        }

        if (npc.MapInstance.IsBlockedZone(positionX, positionY))
        {
            return;
        }

        int distance = npc.Position.GetDistance(positionX, positionY);
        if (distance > 10)
        {
            return;
        }

        npc.ChangePosition(new Position(positionX, positionY));
        session.BroadcastMovement(npc, new ExceptSessionBroadcast(session));
    }
}