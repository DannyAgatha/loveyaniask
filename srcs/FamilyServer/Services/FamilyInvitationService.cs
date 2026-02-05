using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PhoenixLib.Logging;
using WingsAPI.Communication;
using WingsAPI.Communication.Families;
using WingsEmu.Core.Extensions;

namespace FamilyServer.Services
{
    public class FamilyInvitationService : IFamilyInvitationService
    {
        private readonly Dictionary<long, List<FamilyInvitation>> _invitations = new();

        public async ValueTask<EmptyResponse> SaveFamilyInvitationAsync(FamilyInvitationSaveRequest request)
        {
            FamilyInvitation senderInvitation = request.Invitation;

            if (!_invitations.TryGetValue(senderInvitation.SenderId, out List<FamilyInvitation> invitations))
            {
                invitations = new List<FamilyInvitation>();
                _invitations[senderInvitation.SenderId] = invitations;
            }

            invitations.Add(senderInvitation);
            Log.Info($"Family invitation added: sender: {senderInvitation.SenderId} target: {senderInvitation.TargetId}");
            return new EmptyResponse();
        }

        public async ValueTask<FamilyInvitationContainsResponse> ContainsFamilyInvitationAsync(FamilyInvitationRequest request)
        {
            long senderId = request.SenderId;
            long targetId = request.TargetId;

            if (!_invitations.ContainsKey(senderId))
            {
                Log.Debug($"[BOOL] Family invitation not found! Sender: {senderId} Target: {targetId}");
                return new FamilyInvitationContainsResponse
                {
                    IsContains = false
                };
            }

            List<FamilyInvitation> invite = _invitations.GetOrDefault(senderId);
            Log.Debug("[BOOL] Family invitations found!");
            Log.Debug($"[BOOL] Family invitation contain target: {targetId} - {invite.All(x => x.TargetId == targetId)}");
            return new FamilyInvitationContainsResponse
            {
                IsContains = invite.All(x => x.TargetId == targetId)
            };
        }

        public async ValueTask<FamilyInvitationGetResponse> GetFamilyInvitationAsync(FamilyInvitationRequest request) => new()
        {
            Invitation = _invitations.GetOrDefault(request.SenderId)?.FirstOrDefault(x => x.TargetId == request.TargetId)
        };

        public async ValueTask<EmptyResponse> RemoveFamilyInvitationAsync(FamilyInvitationRemoveRequest request)
        {
            _invitations.Remove(request.SenderId);
            return new EmptyResponse();
        }
    }
}