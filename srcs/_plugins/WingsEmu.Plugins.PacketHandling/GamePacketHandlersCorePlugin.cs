using System;
using Microsoft.Extensions.DependencyInjection;
using PhoenixLib.Extensions;
using WingsAPI.Packets;
using WingsAPI.Packets.Handling;
using WingsAPI.Plugins;
using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Battle;
using WingsEmu.Packets;

namespace WingsEmu.Plugins.PacketHandling;

public class GamePacketHandlersCorePlugin : IGameServerPlugin
{
    public string Name => nameof(GamePacketHandlersCorePlugin);

    public void AddDependencies(IServiceCollection services, GameServerLoader gameServer)
    {
        services.AddSingleton(typeof(IPacketHandlerContainer<>), typeof(GenericPacketHandlerContainer<>));
        services.AddClientPacketsInAssembly<ClientPacket>();


        Type[] types = typeof(GamePacketHandlersCorePlugin).Assembly.GetTypesImplementingGenericClass(typeof(GenericGamePacketHandlerBase<>));
        services.AddGamePacketHandlersInAssembly(types);


        services.AddTransient<ISkillExecutor, SkillExecutor>();
        services.AddSingleton<ISkillUsageManager, SkillUsageManager>();
    }
}