using System;
using System.Collections.Generic;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;

namespace Plugin.CoreImpl.Maps.Systems
{
    public sealed class SkillCooldownSystem
    {
        public void Update(IPlayerEntity character, in DateTime date)
        {
            UpdateCharacterCooldowns(character, date);
        }

        private void UpdateCharacterCooldowns(IPlayerEntity character, in DateTime date)
        {
            if (character.SkillCooldowns.IsEmpty && character.MatesSkillCooldowns.IsEmpty)
            {
                return;
            }

            var toProcess = new List<(DateTime time, short castId)>();
            var toProcessMates = new List<(DateTime time, short castId, MateType matetype)>();

            while (character.SkillCooldowns.TryDequeue(out (DateTime time, short castId) cooldown))
            {
                if (cooldown.time > date)
                {
                    toProcess.Add((cooldown.time, cooldown.castId));
                    continue;
                }

                character.Session.SendSkillCooldownReset(cooldown.castId);
            }

            while (character.MatesSkillCooldowns.TryDequeue(out (DateTime time, short castId, MateType mateType) cooldown))
            {
                if (cooldown.time > date)
                {
                    toProcessMates.Add((cooldown.time, cooldown.castId, cooldown.mateType));
                    continue;
                }

                switch (cooldown.mateType)
                {
                    case MateType.Partner:
                        character.Session.SendPartnerSkillCooldown(cooldown.castId);
                        break;
                    case MateType.Pet:
                        character.Session.SendMateSkillCooldownReset(cooldown.castId);
                        break;
                }
            }

            foreach ((DateTime time, short castId) in toProcess)
            {
                character.AddSkillCooldown(time, castId);
            }

            foreach ((DateTime time, short castId, MateType mateType) in toProcessMates)
            {
                character.AddMateSkillCooldown(time, castId, mateType);
            }
        }
    }
}