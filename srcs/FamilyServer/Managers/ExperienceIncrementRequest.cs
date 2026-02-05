namespace FamilyServer.Managers
{
    public class ExperienceIncrementRequest
    {
        public long? FamilyId { get; set; }
        public long? CharacterId { get; set; }
        public long Experience { get; set; }
    }
}