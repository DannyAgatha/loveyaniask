using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Logging;
using WingsAPI.Communication;
using WingsAPI.Communication.Sessions;
using WingsAPI.Communication.Sessions.Model;
using WingsAPI.Communication.Sessions.Request;
using WingsAPI.Communication.Sessions.Response;

namespace WingsEmu.Master.Sessions
{
    public class SessionService : ISessionService
    {
        private static readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
        private readonly EncryptionKeyFactory _encryptionKeyFactory;
        private readonly ISessionManager _sessionManager;

        public SessionService(ISessionManager sessionManager, EncryptionKeyFactory encryptionKeyFactory)
        {
            _sessionManager = sessionManager;
            _encryptionKeyFactory = encryptionKeyFactory;
        }

        public async ValueTask<SessionResponse> CreateSession(CreateSessionRequest request)
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                Session existingSession = await _sessionManager.GetSessionByAccountId(request.AccountId);
                if (existingSession is not null && existingSession.State != SessionState.Disconnected && existingSession.State != SessionState.ServerSelection)
                {
                    Log.Debug($"[SESSION_SERVICE][CREATE_SESSION] A Session for account {request.AccountId} already exists");
                    return new SessionResponse
                    {
                        ResponseType = RpcResponseType.GENERIC_SERVER_ERROR
                    };
                }

                var session = new Session
                {
                    Id = Guid.NewGuid().ToString().Replace("-", ""),
                    IpAddress = request.IpAddress,
                    AccountId = request.AccountId,
                    AccountName = request.AccountName,
                    Authority = request.AuthorityType,
                    State = SessionState.Disconnected,
                    EncryptionKey = _encryptionKeyFactory.CreateEncryptionKey()
                };

                bool created = await _sessionManager.Create(session);
                if (!created)
                {
                    Log.Debug($"[SESSION_SERVICE][CREATE_SESSION] Failed to save session of account {session.AccountId} with session id {session.Id} into redis");
                    return new SessionResponse
                    {
                        ResponseType = RpcResponseType.GENERIC_SERVER_ERROR
                    };
                }

                Log.Debug($"[SESSION_SERVICE][CREATE_SESSION] Successfully created session for account {session.AccountId} with session id {session.Id}");
                return new SessionResponse
                {
                    ResponseType = RpcResponseType.SUCCESS,
                    Session = session
                };
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        public async ValueTask<SessionResponse> GetSessionByAccountName(GetSessionByAccountNameRequest request)
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                Session session = await _sessionManager.GetSessionByAccountName(request.AccountName);
                if (session is null)
                {
                    return new SessionResponse
                    {
                        ResponseType = RpcResponseType.GENERIC_SERVER_ERROR
                    };
                }

                return new SessionResponse
                {
                    ResponseType = RpcResponseType.SUCCESS,
                    Session = session
                };
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        public async ValueTask<SessionResponse> GetSessionByAccountId(GetSessionByAccountIdRequest request)
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                Session session = await _sessionManager.GetSessionByAccountId(request.AccountId);
                if (session is null)
                {
                    Log.Debug($"[SESSION_SERVICE][GET_SESSION_BY_ACCOUNT_ID] Couldn't find session with account id: {request.AccountId}");
                    return new SessionResponse
                    {
                        ResponseType = RpcResponseType.GENERIC_SERVER_ERROR
                    };
                }

                Log.Debug($"[SESSION_SERVICE][GET_SESSION_BY_ACCOUNT_ID] Successfully found a session with account id: {request.AccountId}");
                return new SessionResponse
                {
                    ResponseType = RpcResponseType.SUCCESS,
                    Session = session
                };
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
        
        public async ValueTask<SessionResponse> GetSessionBySessionId(GetSessionBySessionIdRequest request)
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                Session session = await _sessionManager.GetSessionBySessionId(request.SessionId);
                if (session is null)
                {
                    return new SessionResponse
                    {
                        ResponseType = RpcResponseType.GENERIC_SERVER_ERROR
                    };
                }
                
                return new SessionResponse
                {
                    ResponseType = RpcResponseType.SUCCESS,
                    Session = session
                };
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        public async ValueTask<SessionResponse> ConnectToLoginServer(ConnectToLoginServerRequest request)
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                Session session = await _sessionManager.GetSessionByAccountId(request.AccountId);
                if (session is null)
                {
                    Log.Debug($"[SESSION_SERVICE][CONNECT_TO_LOGIN_SERVER] Can't connect account with ID {request.AccountId} to login server (No session found)");
                    return new SessionResponse
                    {
                        ResponseType = RpcResponseType.GENERIC_SERVER_ERROR
                    };
                }

                if (session.State != SessionState.Disconnected && session.State != SessionState.CharacterSelection)
                {
                    Log.Debug($"[SESSION_SERVICE][CONNECT_TO_LOGIN_SERVER] Can't session with ID {request.AccountId} to login server (Already connected)");
                    return new SessionResponse
                    {
                        ResponseType = RpcResponseType.GENERIC_SERVER_ERROR
                    };
                }

                session.State = SessionState.ServerSelection;
                session.HardwareId = request.HardwareId;
                session.ClientVersion = request.ClientVersion;

                bool updated = await _sessionManager.Update(session);
                if (!updated)
                {
                    Log.Debug($"[SESSION_SERVICE][CONNECT_TO_LOGIN_SERVER] Failed to update session of {session.AccountId} into redis");
                    return new SessionResponse
                    {
                        ResponseType = RpcResponseType.GENERIC_SERVER_ERROR
                    };
                }

                Log.Debug($"[SESSION_SERVICE][CONNECT_TO_LOGIN_SERVER] Successfully connected session of {session.AccountId} to login server");
                return new SessionResponse
                {
                    ResponseType = RpcResponseType.SUCCESS,
                    Session = session
                };
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        public async ValueTask<SessionResponse> Disconnect(DisconnectSessionRequest request)
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                Session session = await _sessionManager.GetSessionByAccountId(request.AccountId);
                if (session is null)
                {
                    Log.Debug($"[SESSION_SERVICE][DISCONNECT] Can't disconnect session of account {request.AccountId} (Couldn't find session)");
                    return new SessionResponse
                    {
                        ResponseType = RpcResponseType.GENERIC_SERVER_ERROR
                    };
                }

                if (session.State == SessionState.CrossChannelAuthentication)
                {
                    Log.Debug($"[SESSION_SERVICE][DISCONNECT] Can't disconnect session of account {request.AccountId} (CrossChannel or CharacterSelection)");
                    return new SessionResponse
                    {
                        ResponseType = RpcResponseType.GENERIC_SERVER_ERROR
                    };
                }

                if (request.ForceDisconnect == false)
                {
                    if (session.EncryptionKey != request.EncryptionKey)
                    {
                        Log.Debug($"[SESSION_SERVICE][DISCONNECT] Can't disconnect session of account {request.AccountId} (CrossChannel or CharacterSelection)");
                        return new SessionResponse
                        {
                            ResponseType = RpcResponseType.GENERIC_SERVER_ERROR
                        };
                    }
                }

                session.State = SessionState.Disconnected;
                session.CharacterId = 0;
                session.ChannelId = 0;
                session.ServerGroup = null;

                bool saved = await _sessionManager.Update(session);
                if (!saved)
                {
                    Log.Debug($"[SESSION_SERVICE][DISCONNECT] Failed to update session of account {request.AccountId}");
                    return new SessionResponse
                    {
                        ResponseType = RpcResponseType.GENERIC_SERVER_ERROR
                    };
                }

                Log.Debug($"[SESSION_SERVICE][DISCONNECT] Successfully updated session of account {request.AccountId}");
                return new SessionResponse
                {
                    ResponseType = RpcResponseType.SUCCESS
                };
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        public async ValueTask<SessionResponse> ConnectCharacter(ConnectCharacterRequest request)
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                Session session = await _sessionManager.GetSessionByAccountId(request.AccountId);
                if (session is null || session.State != SessionState.CharacterSelection || session.ChannelId != request.ChannelId)
                {
                    Log.Debug($"[SESSION_SERVICE][CONNECT_CHARACTER] Can't connect character from session of account {request.AccountId} (Couldn't find session)");
                    return new SessionResponse
                    {
                        ResponseType = RpcResponseType.GENERIC_SERVER_ERROR
                    };
                }

                session.CharacterId = request.CharacterId;
                session.State = SessionState.InGame;

                bool updated = await _sessionManager.Update(session);
                if (!updated)
                {
                    Log.Debug($"[SESSION_SERVICE][CONNECT_CHARACTER] Failed to update session of account {request.AccountId}");
                    return new SessionResponse
                    {
                        ResponseType = RpcResponseType.GENERIC_SERVER_ERROR
                    };
                }

                Log.Debug($"[SESSION_SERVICE][CONNECT_CHARACTER] Successfully update session of account {request.AccountId}");
                return new SessionResponse
                {
                    ResponseType = RpcResponseType.SUCCESS,
                    Session = session
                };
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        public async ValueTask<SessionResponse> ActivateCrossChannelAuthentication(ActivateCrossChannelAuthenticationRequest request)
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                Session session = await _sessionManager.GetSessionByAccountId(request.AccountId);
                if (session is null || session.State != SessionState.InGame)
                {
                    Log.Debug($"[SESSION_SERVICE][ACTIVE_CROSS_CHANNEL_AUTHENTICATION] Couldn't find session for account {request.AccountId} or account doesn't have correct state ({session?.State})");
                    return new SessionResponse
                    {
                        ResponseType = RpcResponseType.GENERIC_SERVER_ERROR
                    };
                }

                session.State = SessionState.CrossChannelAuthentication;
                session.AllowedCrossChannelId = request.ChannelId;

                bool updated = await _sessionManager.Update(session);
                if (!updated)
                {
                    Log.Debug($"[SESSION_SERVICE][ACTIVE_CROSS_CHANNEL_AUTHENTICATION] Failed to update session of account {request.AccountId}");
                    return new SessionResponse
                    {
                        ResponseType = RpcResponseType.GENERIC_SERVER_ERROR
                    };
                }


                Log.Debug($"[SESSION_SERVICE][ACTIVE_CROSS_CHANNEL_AUTHENTICATION] Successfully updated session of account {request.AccountId}");
                return new SessionResponse
                {
                    ResponseType = RpcResponseType.SUCCESS,
                    Session = session
                };
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        public async ValueTask<SessionResponse> Pulse(PulseRequest request)
        {
            Log.Debug($"[SESSION_SERVICE][PULSE] Starting pulse for account {request.AccountId}");
            await _semaphoreSlim.WaitAsync();
            try
            {
                Session session = await _sessionManager.GetSessionByAccountId(request.AccountId);
                if (session is null)
                {
                    Log.Warn($"[SESSION_SERVICE][PULSE] Session not found for account {request.AccountId}");
                    return new SessionResponse
                    {
                        ResponseType = RpcResponseType.SESSION_NOT_FOUND
                    };
                }

                if (session.State == null)
                {
                    Log.Debug($"[SESSION_SERVICE][PULSE] Null state detected for session {session.Id}. Attempting recovery...");
                    session.State = SessionState.InGame;
                    await _sessionManager.Update(session);
                }

                if (session.State != SessionState.InGame)
                {
                    Log.Warn($"[SESSION_SERVICE][PULSE] Invalid session state for account {request.AccountId}. State: {session.State}");
                    return new SessionResponse
                    {
                        ResponseType = RpcResponseType.INVALID_SESSION_STATE
                    };
                }

                bool pulsed = await _sessionManager.Pulse(session);
                if (!pulsed)
                {
                    Log.Warn($"[SESSION_SERVICE][PULSE] Failed to pulse session for account {request.AccountId}");
                    return new SessionResponse
                    {
                        ResponseType = RpcResponseType.PULSE_FAILED
                    };
                }

                session.LastPulse = DateTime.UtcNow;
                bool updated = await _sessionManager.Update(session);
                if (!updated)
                {
                    Log.Warn($"[SESSION_SERVICE][PULSE] Failed to update session for account {request.AccountId}");
                    return new SessionResponse
                    {
                        ResponseType = RpcResponseType.UPDATE_FAILED
                    };
                }

                Log.Debug($"[SESSION_SERVICE][PULSE] Successfully pulsed and updated session for account {request.AccountId}");
                return new SessionResponse
                {
                    ResponseType = RpcResponseType.SUCCESS,
                    Session = session
                };
            }
            catch (Exception ex)
            {
                Log.Error($"[SESSION_SERVICE][PULSE] Unhandled exception during pulse for account {request.AccountId}", ex);
                return new SessionResponse
                {
                    ResponseType = RpcResponseType.UNHANDLED_ERROR
                };
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        public async ValueTask<SessionResponse> ConnectToWorldServer(ConnectToWorldServerRequest request)
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                Session session = await _sessionManager.GetSessionByAccountId(request.AccountId);
                if (session is null)
                {
                    Log.Debug($"[SESSION_SERVICE][CONNECT_TO_WORLD_SERVER] Couldn't find session of account or incorrect state {request.AccountId}");
                    return new SessionResponse
                    {
                        ResponseType = RpcResponseType.GENERIC_SERVER_ERROR
                    };
                }

                // Can't connect to world if not in server selection screen or cross channel
                if (session.State != SessionState.ServerSelection && session.State != SessionState.CrossChannelAuthentication)
                {
                    Log.Debug($"[SESSION_SERVICE][CONNECT_TO_WORLD_SERVER] Incorrect session state {session.State}");
                    return new SessionResponse
                    {
                        ResponseType = RpcResponseType.GENERIC_SERVER_ERROR
                    };
                }

                if (session.State == SessionState.CrossChannelAuthentication && session.AllowedCrossChannelId != request.ChannelId)
                {
                    Log.Debug($"[SESSION_SERVICE][CONNECT_TO_WORLD_SERVER] Incorrect cross channel ID {request.ChannelId} instead of {session.AllowedCrossChannelId}");
                    return new SessionResponse
                    {
                        ResponseType = RpcResponseType.GENERIC_SERVER_ERROR
                    };
                }

                session.LastChannelId = session.ChannelId;
                session.ServerGroup = request.ServerGroup;
                session.ChannelId = request.ChannelId;
                session.State = SessionState.CharacterSelection;
                session.AllowedCrossChannelId = 0;

                bool updated = await _sessionManager.Update(session);
                if (!updated)
                {
                    Log.Debug($"[SESSION_SERVICE][CONNECT_TO_WORLD_SERVER] Failed to update session of account {session.AccountId}");
                    return new SessionResponse
                    {
                        ResponseType = RpcResponseType.GENERIC_SERVER_ERROR
                    };
                }

                Log.Debug($"[SESSION_SERVICE][CONNECT_TO_WORLD_SERVER] Successfully updated session of account {session.AccountId}");
                return new SessionResponse
                {
                    ResponseType = RpcResponseType.SUCCESS,
                    Session = session
                };
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
    }
}