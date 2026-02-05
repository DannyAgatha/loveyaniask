using WingsAPI.Data.Families;
using WingsAPI.Packets.Enums.Families;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Families;

namespace WingsAPI.Game.Extensions.Families
{
    public static class FamilyWarehouseExtensions
    {
        public static bool CheckLogHistoryPermission(this IClientSession session)
        {
            return session.PlayerEntity.GetFamilyAuthority() switch
            {
                FamilyAuthority.Keeper => session.PlayerEntity.Family.AssistantCanGetHistory,
                FamilyAuthority.Member => session.PlayerEntity.Family.MemberCanGetHistory,
                _ => true
            };
        }

        public static bool CheckLogHistoryPermission(this FamilyMembershipDto membershipDto, FamilyDTO family)
        {
            return membershipDto.Authority switch
            {
                FamilyAuthority.Keeper => family.AssistantCanGetHistory,
                FamilyAuthority.Member => family.MemberCanGetHistory,
                _ => true
            };
        }

        public static bool CheckPutWithdrawPermission(this IClientSession session, FamilyWarehouseAuthorityType authorityRequested)
        {
            FamilyWarehouseAuthorityType memberAuthority = session.PlayerEntity.GetFamilyAuthority() switch
            {
                FamilyAuthority.Member => session.PlayerEntity.Family.MemberWarehouseAuthorityType,
                FamilyAuthority.Keeper => session.PlayerEntity.Family.AssistantWarehouseAuthorityType,
                _ => FamilyWarehouseAuthorityType.PutAndWithdraw
            };

            return memberAuthority.CheckPutWithdrawPermission(authorityRequested);
        }

        public static bool CheckPutWithdrawPermission(this FamilyMembershipDto membershipDto, FamilyDTO family, FamilyWarehouseAuthorityType authorityRequested)
        {
            FamilyWarehouseAuthorityType memberAuthority = membershipDto.Authority switch
            {
                FamilyAuthority.Member => family.MemberWarehouseAuthorityType,
                FamilyAuthority.Keeper => family.AssistantWarehouseAuthorityType,
                _ => FamilyWarehouseAuthorityType.PutAndWithdraw
            };

            return memberAuthority.CheckPutWithdrawPermission(authorityRequested);
        }

        public static bool CheckPutWithdrawPermission(this FamilyWarehouseAuthorityType memberAuthority, FamilyWarehouseAuthorityType authorityRequested) => authorityRequested <= memberAuthority;
    }
}