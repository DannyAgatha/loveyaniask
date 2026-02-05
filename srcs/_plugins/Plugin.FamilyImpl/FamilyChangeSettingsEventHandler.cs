using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Communication;
using WingsAPI.Communication.Families;
using WingsAPI.Game.Extensions.Families;
using WingsAPI.Packets.Enums.Families;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Families;

namespace Plugin.FamilyImpl
{
    public class FamilyChangeSettingsEventHandler : IAsyncEventProcessor<FamilyChangeSettingsEvent>
    {
        private readonly IFamilyService _familyService;
        private readonly IGameLanguageService _gameLanguage;

        public FamilyChangeSettingsEventHandler(IGameLanguageService gameLanguage, IFamilyService familyService)
        {
            _gameLanguage = gameLanguage;
            _familyService = familyService;
        }

        public async Task HandleAsync(FamilyChangeSettingsEvent e, CancellationToken cancellation)
        {
            IClientSession session = e.Sender;

            if (!session.PlayerEntity.IsInFamily())
            {
                session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.FAMILY_INFO_NO_FAMILY, session.UserLanguage));
                return;
            }

            if (!session.PlayerEntity.IsHeadOfFamily() && session.PlayerEntity.GetFamilyAuthority() != FamilyAuthority.Deputy)
            {
                session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.FAMILY_INFO_NO_FAMILY_RIGHT, session.UserLanguage));
                return;
            }

            IFamily family = session.PlayerEntity.Family;
            FamilyActionType actionType = e.FamilyActionType;
            byte value = e.Value;

            bool familyOption = false;
            bool option = value == 1;

            switch (e.Authority)
            {
                case FamilyAuthority.Keeper:
                    switch (actionType)
                    {
                        case FamilyActionType.FamilyWarehouse:
                            if (!Enum.TryParse(value.ToString(), out FamilyWarehouseAuthorityType authorityType))
                            {
                                return;
                            }

                            if (family.AssistantWarehouseAuthorityType == authorityType)
                            {
                                return;
                            }

                            option = true;

                            break;
                        default:
                            familyOption = actionType switch
                            {
                                FamilyActionType.SendInvite => family.AssistantCanInvite,
                                FamilyActionType.Notice => family.AssistantCanNotice,
                                FamilyActionType.FamilyShout => family.AssistantCanShout,
                                FamilyActionType.FamilyWarehouseHistory => family.AssistantCanGetHistory,
                                _ => option
                            };
                            break;
                    }

                    break;

                case FamilyAuthority.Member:
                    switch (actionType)
                    {
                        case FamilyActionType.Notice: // Member Warehouse Authority
                            if (!Enum.TryParse(value.ToString(), out FamilyWarehouseAuthorityType authorityType))
                            {
                                return;
                            }

                            if (family.MemberWarehouseAuthorityType == authorityType)
                            {
                                return;
                            }

                            option = true;

                            break;
                        default:
                            if (actionType != FamilyActionType.SendInvite) // Member History
                            {
                                return;
                            }

                            familyOption = family.MemberCanGetHistory;
                            break;
                    }

                    break;
                default:
                    return;
            }

            if (familyOption == option)
            {
                return;
            }

            BasicRpcResponse response = await _familyService.UpdateFamilySettingsAsync(new FamilySettingsRequest
            {
                FamilyId = family.Id,
                Authority = e.Authority,
                FamilyActionType = e.FamilyActionType,
                Value = value
            });

            if (response.ResponseType != RpcResponseType.SUCCESS)
            {
                return;
            }

            if (e.Authority == FamilyAuthority.Member)
            {
                switch (actionType)
                {
                    case FamilyActionType.SendInvite:
                        await session.FamilyAddLogAsync(FamilyLogType.RightChanged, session.PlayerEntity.Name, ((byte)e.Authority).ToString(),
                            ((byte)FamilyActionType.FamilyWarehouseHistory + 1).ToString(), value.ToString());
                        break;
                    case FamilyActionType.Notice:
                        await session.FamilyAddLogAsync(FamilyLogType.RightChanged, session.PlayerEntity.Name, ((byte)e.Authority).ToString(), ((byte)FamilyActionType.FamilyWarehouse + 1).ToString(),
                            value.ToString());
                        break;
                }

                return;
            }

            await session.FamilyAddLogAsync(FamilyLogType.RightChanged, session.PlayerEntity.Name, ((byte)e.Authority).ToString(), ((byte)e.FamilyActionType + 1).ToString(), value.ToString());
        }
    }
}