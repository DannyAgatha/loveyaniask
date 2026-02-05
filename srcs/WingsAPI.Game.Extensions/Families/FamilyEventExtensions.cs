// NosEmu
// 


using System;
using System.Threading.Tasks;
using WingsAPI.Data.Families;
using WingsEmu.Game.Families.Enum;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Families;

namespace WingsAPI.Game.Extensions.Families
{
    public static class FamilyEventExtensions
    {
        public static async Task FamilyAddLogAsync(this IClientSession session, FamilyLogType familyLogType, string actor, string firstArg = null, string secondArg = null, string thirdArg = null)
        {
            if (!session.PlayerEntity.IsInFamily())
            {
                return;
            }

            var familyLog = new FamilyLogDto
            {
                FamilyId = session.PlayerEntity.Family.Id,
                FamilyLogType = familyLogType,
                Actor = actor,
                Argument1 = firstArg,
                Argument2 = secondArg,
                Argument3 = thirdArg,
                Timestamp = DateTime.UtcNow
            };

            await session.EmitEventAsync(new FamilyAddLogEvent(familyLog));
        }

        public static async Task FamilyAddExperience(this IClientSession session, int experience, FamXpObtainedFromType type)
        {
            if (!session.PlayerEntity.IsInFamily())
            {
                return;
            }

            await session.EmitEventAsync(new FamilyAddExperienceEvent(experience, type));
        }
    }
}