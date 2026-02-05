using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Plugin.FamilyImpl.Achievements;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Alzanor.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Game.Alzanor
{
    public interface IAlzanorManager
    {
        bool IsActive { get; set; }
        bool IsRegistrationActive { get; }
        DateTime RegistrationStartTime { get; }
        DateTime? AlzanorProcessTime { get; set; }
        IReadOnlyList<(TimeSpan, int, TimeType)> Warnings { get; }
        IEnumerable<long> RegisteredPlayers { get; }
        IEnumerable<AlzanorParty> AlzanorParties { get; }

        void EnableAlzanorRegistration();
        void DisableAlzanorRegistration();
        void RegisterPlayer(long id);
        void UnregisterPlayer(long id);
        void ClearRegisteredPlayers();
        void AddAlzanor(AlzanorParty alzanorParty);
        void RemoveAlzanor(AlzanorParty alzanorParty);
        IMonsterEntity GetAlzanorBoss();
        IMonsterEntity AlzanorBoss { get; set; }
        void RefreshAlzanorInstance();
        DateTime AlzanorStart { get; set; }
        DateTime AlzanorEnd { get; set; }
        int RedDamage { get; set; }
        int BlueDamage { get; set; }
        IMapInstance GetAlzanorInstance();
        IMapInstance AlzanorInstance { get; set; }
        void ClearEverything();
        List<AlzanorEventStats> AlzanorEventStats { get; set; }
        void IncreaseKillDeathStats(IClientSession session, bool isKill);
        AlzanorEventStats GetAlzanorEventStats(IClientSession session);
        List<AlzanorEventStats> GetTopPlayers();
    }

    public class AlzanorEventStats
    {
        public IClientSession Player { get; set; }
        public short Kills { get; set; }
        public short Deaths { get; set; }
        public short Points => (short)(Kills - Deaths);
    }

    public class AlzanorManager : IAlzanorManager
    {
        private readonly ISessionManager _sessionManager;
        private readonly AlzanorConfiguration _alzanorConfiguration;

        private readonly ReaderWriterLockSlim _lock = new();
        private readonly ConcurrentDictionary<Guid, AlzanorParty> _alzanorParties = new();
        private readonly HashSet<long> _registeredPlayers = new();

        public IMonsterEntity AlzanorBoss { get; set; }
        public IMapInstance AlzanorInstance { get; set; }

        public bool IsActive { get; set; }
        public bool IsRegistrationActive { get; private set; }
        public DateTime RegistrationStartTime { get; private set; }
        public DateTime? AlzanorProcessTime { get; set; }
        public IReadOnlyList<(TimeSpan, int, TimeType)> Warnings { get; }
        public IEnumerable<AlzanorParty> AlzanorParties => _alzanorParties.Values.ToArray();
        public DateTime AlzanorStart { get; set; }
        public DateTime AlzanorEnd { get; set; }
        public int RedDamage { get; set; }
        public int BlueDamage { get; set; }

        public AlzanorManager(AlzanorConfiguration alzanorConfiguration, ISessionManager sessionManager)
        {
            _alzanorConfiguration = alzanorConfiguration;
            _sessionManager = sessionManager;

            var warnings = new List<(TimeSpan, int, TimeType)>();
            foreach (TimeSpan warning in alzanorConfiguration.Warnings)
            {
                TimeSpan time = TimeSpan.FromMinutes(5) - warning;
                bool isSec = time.TotalMinutes < 1;
                warnings.Add((warning, (int)(isSec ? time.TotalSeconds : time.TotalMinutes), isSec ? TimeType.SECONDS : TimeType.MINUTES));
            }

            Warnings = warnings;
        }

        #region Registration Methods

        public void EnableAlzanorRegistration()
        {
            IsRegistrationActive = true;
            RegistrationStartTime = DateTime.UtcNow.AddSeconds(30);
        }

        public void DisableAlzanorRegistration()
        {
            IsRegistrationActive = false;
            RegistrationStartTime = DateTime.MinValue;
        }

        public void RegisterPlayer(long id)
        {
            _lock.EnterWriteLock();
            try
            {
                _registeredPlayers.Add(id);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void UnregisterPlayer(long id)
        {
            _lock.EnterWriteLock();
            try
            {
                _registeredPlayers.Remove(id);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public IEnumerable<long> RegisteredPlayers
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _registeredPlayers.ToArray();
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        public void ClearRegisteredPlayers()
        {
            _lock.EnterWriteLock();
            try
            {
                _registeredPlayers.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        #endregion

        #region Alzanor Party Management

        public void AddAlzanor(AlzanorParty alzanorParty)
        {
            _alzanorParties.TryAdd(alzanorParty.Id, alzanorParty);
        }

        public void RemoveAlzanor(AlzanorParty alzanorParty)
        {
            _alzanorParties.TryRemove(alzanorParty.Id, out _);
        }

        #endregion

        #region Boss and Instance Management

        public IMonsterEntity GetAlzanorBoss() => AlzanorBoss;

        public IMapInstance GetAlzanorInstance() => AlzanorInstance;

        public void RefreshAlzanorInstance()
        {
            DateTime currentTime = DateTime.UtcNow;
            TimeSpan timeLeft = AlzanorEnd - currentTime;

            if (AlzanorInstance == null)
            {
                return;
            }

            if (AlzanorBoss == null)
            {
                return;
            }

            int maxHp = AlzanorBoss.MaxHp;
            string packet = UiPacketExtension.GenerateChdm(maxHp, RedDamage, BlueDamage, (int)timeLeft.TotalSeconds);
            _sessionManager.Broadcast(packet, new InMapVnumBroadcast(154));
        }

        #endregion

        public void ClearEverything()
        {
            AlzanorBoss = null;
            AlzanorInstance = null;
            AlzanorStart = DateTime.MinValue;
            AlzanorEnd = DateTime.MinValue;
            RedDamage = 0;
            BlueDamage = 0;
            IsActive = false;
            IsRegistrationActive = false;
            RegistrationStartTime = DateTime.MinValue;
            AlzanorProcessTime = null;
            AlzanorEventStats = new();
            ClearRegisteredPlayers();
        }

        public List<AlzanorEventStats> AlzanorEventStats { get; set; } = new();
        public void IncreaseKillDeathStats(IClientSession session, bool isKill)
        {
            AlzanorEventStats stats = GetAlzanorEventStats(session);
            if (isKill)
            {
                stats.Kills += _alzanorConfiguration.PointsPerKill;
            }
            else
            {

                stats.Deaths += _alzanorConfiguration.MinusPointsPerDeath;
            }
        }

        public AlzanorEventStats GetAlzanorEventStats(IClientSession session)
        {
            AlzanorEventStats stats = AlzanorEventStats.FirstOrDefault(x => x.Player == session);
            if (stats == null)
            {
                stats = new AlzanorEventStats
                {
                    Player = session
                };
                AlzanorEventStats.Add(stats);
            }

            return stats;
        }

        public List<AlzanorEventStats> GetTopPlayers()
        {
            return AlzanorEventStats
                .Where(x => x.Points > 0)
                .OrderByDescending(x => x.Points)
                .ThenByDescending(x => x.Kills)
                .ThenBy(x => x.Deaths)
                .ThenByDescending(x => x.Player.PlayerEntity.Reput)
                .Take(5)
                .ToList();
        }

    }
}
