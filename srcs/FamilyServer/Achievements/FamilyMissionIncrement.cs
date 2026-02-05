namespace FamilyServer.Achievements
{
    public class FamilyMissionIncrement
    {
        public long FamilyId { get; set; }
        public long? CharacterId { get; set; }
        public int MissionId { get; set; }
        public int ValueToAdd { get; set; }
    }
}