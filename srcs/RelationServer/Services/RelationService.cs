using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PhoenixLib.Logging;
using PhoenixLib.ServiceBus;
using WingsAPI.Communication;
using WingsAPI.Communication.Relation;
using WingsAPI.Data.Character;
using WingsEmu.DTOs.Relations;
using WingsEmu.Packets.Enums.Relations;
using WingsEmu.Plugins.DistributedGameEvents.Relation;

namespace RelationServer.Services
{
    public class RelationService : IRelationService
    {
        private readonly ICharacterDAO _characterDao;
        private readonly IMessagePublisher<RelationCharacterAddMessage> _messagePublisher;
        private readonly IMessagePublisher<RelationCharacterRemoveMessage> _messagePublisherRemove;
        private readonly ICharacterRelationDAO _relationDao;

        public RelationService(ICharacterRelationDAO relationDao, ICharacterDAO characterDao, IMessagePublisher<RelationCharacterAddMessage> messagePublisher,
            IMessagePublisher<RelationCharacterRemoveMessage> messagePublisherRemove)
        {
            _relationDao = relationDao;
            _characterDao = characterDao;
            _messagePublisher = messagePublisher;
            _messagePublisherRemove = messagePublisherRemove;
        }

        public async Task<RelationAddResponse> AddRelationAsync(RelationAddRequest request)
        {
            long characterId = request.CharacterId;
            long targetId = request.TargetId;
            CharacterRelationType relationType = request.RelationType;
            string characterName = request.CharacterName;

            CharacterDTO target;
            try
            {
                target = _characterDao.GetById(targetId);
            }
            catch (Exception e)
            {
                Log.Error("TargetAddRelationAsync", e);
                return new RelationAddResponse
                {
                    ResponseType = RpcResponseType.UNKNOWN_ERROR
                };
            }

            if (target == null)
            {
                return new RelationAddResponse
                {
                    ResponseType = RpcResponseType.UNKNOWN_ERROR
                };
            }

            var senderRelation = new CharacterRelationDTO
            {
                CharacterId = characterId,
                RelatedCharacterId = targetId,
                RelationType = relationType,
                RelatedName = target.Name
            };

            CharacterRelationDTO targetRelation = null;

            if (relationType != CharacterRelationType.Blocked)
            {
                targetRelation = new CharacterRelationDTO
                {
                    CharacterId = targetId,
                    RelatedCharacterId = characterId,
                    RelationType = relationType,
                    RelatedName = characterName
                };
            }

            try
            {
                await _relationDao.SaveRelationsByCharacterIdAsync(characterId, senderRelation);
            }
            catch (Exception e)
            {
                Log.Error("AddRelationAsync", e);
                return new RelationAddResponse
                {
                    ResponseType = RpcResponseType.UNKNOWN_ERROR
                };
            }

            if (targetRelation == null)
            {
                await _messagePublisher.PublishAsync(new RelationCharacterAddMessage
                {
                    SenderRelation = senderRelation,
                    TargetRelation = null
                });

                return new RelationAddResponse
                {
                    ResponseType = RpcResponseType.SUCCESS,
                    SenderRelation = senderRelation,
                    TargetRelation = null
                };
            }

            try
            {
                await _relationDao.SaveRelationsByCharacterIdAsync(targetId, targetRelation);
            }
            catch (Exception e)
            {
                Log.Error("AddRelationAsync", e);
                return new RelationAddResponse
                {
                    ResponseType = RpcResponseType.UNKNOWN_ERROR
                };
            }

            await _messagePublisher.PublishAsync(new RelationCharacterAddMessage
            {
                SenderRelation = senderRelation,
                TargetRelation = targetRelation
            });

            return new RelationAddResponse
            {
                ResponseType = RpcResponseType.SUCCESS,
                SenderRelation = senderRelation,
                TargetRelation = targetRelation
            };
        }

        public async Task<RelationGetAllResponse> GetRelationsByIdAsync(RelationGetAllRequest request)
        {
            List<CharacterRelationDTO> dtos;
            try
            {
                dtos = await _relationDao.LoadRelationsByCharacterIdAsync(request.CharacterId);
            }
            catch (Exception e)
            {
                Log.Error("[RELATION_SERVICE] Unexpected error: ", e);
                return new RelationGetAllResponse
                {
                    ResponseType = RpcResponseType.GENERIC_SERVER_ERROR
                };
            }

            return new RelationGetAllResponse
            {
                ResponseType = RpcResponseType.SUCCESS,
                CharacterRelationDtos = dtos
            };
        }

        public async Task<BasicRpcResponse> RemoveRelationAsync(RelationRemoveRequest request)
        {
            long characterId = request.CharacterId;
            long targetId = request.TargetId;
            CharacterRelationType relationType = request.RelationType;
            CharacterRelationDTO senderRelation = await _relationDao.GetRelationByCharacterIdAsync(characterId, targetId);

            if (senderRelation == null)
            {
                return new BasicRpcResponse
                {
                    ResponseType = RpcResponseType.UNKNOWN_ERROR
                };
            }

            if (senderRelation.RelationType != relationType)
            {
                return new BasicRpcResponse
                {
                    ResponseType = RpcResponseType.UNKNOWN_ERROR
                };
            }

            try
            {
                await _relationDao.RemoveRelationAsync(senderRelation);
            }
            catch (Exception e)
            {
                Log.Error("RemoveRelationAsync", e);
                return new BasicRpcResponse
                {
                    ResponseType = RpcResponseType.UNKNOWN_ERROR
                };
            }

            if (relationType != CharacterRelationType.Blocked)
            {
                CharacterRelationDTO targetRelation = await _relationDao.GetRelationByCharacterIdAsync(targetId, characterId);
                try
                {
                    await _relationDao.RemoveRelationAsync(targetRelation);
                }
                catch (Exception e)
                {
                    Log.Error("RemoveRelationAsync", e);
                    return new BasicRpcResponse
                    {
                        ResponseType = RpcResponseType.UNKNOWN_ERROR
                    };
                }
            }

            await _messagePublisherRemove.PublishAsync(new RelationCharacterRemoveMessage
            {
                RelationType = relationType,
                CharacterId = characterId,
                TargetId = targetId
            });

            return new BasicRpcResponse
            {
                ResponseType = RpcResponseType.SUCCESS
            };
        }
    }
}