using System.Collections.Generic;
using System.Threading.Tasks;
using PhoenixLib.MultiLanguage;
using WingsAPI.Communication.Sessions.Model;
using WingsEmu.Game._i18n;
using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Maps;
using WingsEmu.Packets;

namespace WingsEmu.Game.Networking;

public interface IClientSession : IUserLanguageService
{
    RegionLanguageType UserLanguage { get; }
    Account Account { get; }
    IPlayerEntity PlayerEntity { get; }
    IMapInstance CurrentMapInstance { get; set; }
    bool HasCurrentMapInstance { get; }
    bool HasSelectedCharacter { get; }
    byte? SelectedCharacterSlot { get; set; }
    string IpAddress { get; }
    string HardwareId { get; }
    string ClientVersion { get; }
    bool IsAuthenticated { get; }
    bool IsConnected { get; }
    bool IsDisposing { get; }
    int SessionId { get; }
    bool DebugMode { get; set; }
    bool GmMode { get; set; }
    void ForceDisconnect();
    void InitializeAccount(Account account, Session session);
    void InitializePlayerEntity(IPlayerEntity character);

    /*
     * Events
     */
    void EmitEvent<T>(T e) where T : PlayerEvent;
    Task EmitEventAsync<T>(T e) where T : PlayerEvent;

    /*
     * Game Network
     */
    void SendPacket<T>(T packet) where T : IPacket;
    void SendPacket(string packet);
    void SendPackets(IEnumerable<string> packets);
}