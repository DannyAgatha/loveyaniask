namespace WingsEmu.Health
{
    public interface IMaintenanceManager
    {
        string ServiceName { get; }
        bool IsMaintenanceActive { get; }
        void ActivateMaintenance();
        void DeactivateMaintenance();
    }
}