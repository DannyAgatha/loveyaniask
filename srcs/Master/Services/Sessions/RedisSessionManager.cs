using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PhoenixLib.Logging;
using Polly;
using StackExchange.Redis;
using WingsAPI.Communication.Sessions.Model;

namespace WingsEmu.Master.Sessions;

public class RedisSessionManager : ISessionManager
{
    private const string SessionPrefix = "session";
    private const string SessionStatePrefix = "session:state";
    private const string SessionMappingPrefix = "session:mapping";
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(4);

    private readonly IDatabase _db;
    private readonly IAsyncPolicy _retryPolicy;

    public RedisSessionManager(IConnectionMultiplexer multiplexer)
    {
        _db = multiplexer.GetDatabase(0);
        _retryPolicy = Policy
            .Handle<RedisException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                new[]
                {
                    TimeSpan.FromSeconds(1),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(4)
                },
                (exception, timeSpan, retryCount, context) =>
                {
                    Log.Warn($"Retry attempt {retryCount} after {timeSpan.TotalSeconds} seconds due to: {exception.Message}");
                }
            );

        // Perform initial health check
        if (!IsRedisHealthy().GetAwaiter().GetResult())
        {
            throw new InvalidOperationException("Redis is not healthy. Unable to initialize RedisSessionManager.");
        }

        // Set up event handlers for connection status changes
        multiplexer.ConnectionFailed += (sender, args) =>
        {
            Log.Error($"Redis connection failed: ", args.Exception);
        };

        multiplexer.ConnectionRestored += (sender, args) =>
        {
            Log.Warn("Redis connection restored");
        };
        multiplexer.ErrorMessage += (sender, args) =>
        {
            Log.Warn($"Redis error message: {args.Message}");
        };
        multiplexer.InternalError += (sender, args) =>
        {
            Log.Error($"Redis internal error: ", args.Exception);
        };
    }

    private async Task<bool> IsRedisHealthy()
    {
        try
        {
            TimeSpan result = await _db.PingAsync();
            bool isHealthy = result < TimeSpan.FromSeconds(1);
            
            if (isHealthy)
            {
                Log.Debug("Redis health check passed");
            }
            else
            {
                Log.Warn($"Redis health check failed. Ping time: {result.TotalMilliseconds}ms");
            }
            
            return isHealthy;
        }
        catch (Exception ex)
        {
            Log.Error("Redis health check failed", ex);
            return false;
        }
    }

    public async Task<bool> Create(Session session)
    {
        
        
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            try
            {
                if (!await IsRedisHealthy())
                {
                    Log.Warn($"--------- Redis is unhealthy. Unable to create session: {session.Id}");
                }
                
                Log.Debug($"Attempting to create session for account {session.AccountId}");
                ITransaction transaction = _db.CreateTransaction();
                Task<bool> setSessionTask = transaction.StringSetAsync(CreateSessionKey(session.Id), JsonConvert.SerializeObject(session), Ttl);
                Task<bool> setStateTask = transaction.StringSetAsync(CreateSessionStateKey(session.Id), session.State.ToString(), Ttl);
                Task<bool> mappingTask1 = transaction.StringSetAsync(CreateAccountIdMappingKey(session.AccountId), session.Id, Ttl);
                Task<bool> mappingTask2 = transaction.StringSetAsync(CreateAccountNameMappingKey(session.AccountName), session.Id, Ttl);

                bool committed = await transaction.ExecuteAsync();
                bool result = committed && await setSessionTask && await setStateTask && await mappingTask1 && await mappingTask2;
                //Log.Debug($"Session creation for account {session.AccountId} {(result ? "succeeded" : "failed")}");
                if (result)
                {
                    Log.Debug($"Session creation for account {session.AccountId} succeeded.");
                }
                else
                {
                    Log.Warn($"Session retrieval for account {session.AccountId} failed.");
                }
                return result;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to create session for account {session.AccountId}", ex);
                return false;
            }
        });
    }

    public async Task<bool> Update(Session session)
    {
        
        
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            try
            {
                if (!await IsRedisHealthy())
                {
                    Log.Warn($"--------- Redis is unhealthy. Unable to update session: {session.Id}");
                }
                Log.Debug($"Attempting to update session for account {session.AccountId}");
                ITransaction transaction = _db.CreateTransaction();
                Task<bool> setSessionTask = transaction.StringSetAsync(CreateSessionKey(session.Id), JsonConvert.SerializeObject(session), Ttl);
                Task<bool> setStateTask = transaction.StringSetAsync(CreateSessionStateKey(session.Id), session.State.ToString(), Ttl);
                Task<bool> mappingTask1 = transaction.StringSetAsync(CreateAccountIdMappingKey(session.AccountId), session.Id, Ttl);
                Task<bool> mappingTask2 = transaction.StringSetAsync(CreateAccountNameMappingKey(session.AccountName), session.Id, Ttl);

                bool committed = await transaction.ExecuteAsync();
                bool result = committed && await setSessionTask && await setStateTask && await mappingTask1 && await mappingTask2;
                //Log.Debug($"Session update for account {session.AccountId} {(result ? "succeeded" : "failed")}");
                if (result)
                {
                    Log.Debug($"Session retrieval for account {session.AccountId} succeeded.");
                }
                else
                {
                    Log.Warn($"Session retrieval for account {session.AccountId} failed.");
                }
                return result;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to update session for account {session.AccountId}", ex);
                return false;
            }
        });
    }

    public async Task<Session> GetSessionByAccountName(string accountName)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            try
            {
                if (!await IsRedisHealthy())
                {
                    Log.Warn($"--------- Redis is unhealthy. Unable to get session for account name: {accountName}");
                }
                
                Log.Debug($"Attempting to get session for account name {accountName}");
                string sessionId = await _db.StringGetAsync(CreateAccountNameMappingKey(accountName));
                if (string.IsNullOrEmpty(sessionId))
                {
                    Log.Debug($"No session found for account name {accountName}");
                    return null;
                }

                Session session = await GetSessionById(sessionId);
                //Log.Debug($"Session retrieval for account name {accountName} {(session != null ? "succeeded" : "failed")}");
                if (session != null )
                {
                    Log.Debug($"Session retrieval for account name {accountName} succeeded.");
                }
                else
                {
                    Log.Warn($"Session retrieval for account name {accountName} failed.");
                }
                return session;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to get session for account name {accountName}", ex);
                return null;
            }
        });
    }
    
    public async Task<Session> GetSessionBySessionId(string sessionId)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            try
            {
                if (!await IsRedisHealthy())
                {
                    Log.Warn($"--------- Redis is unhealthy. Unable to get session for session ID {sessionId}");
                }
                Log.Debug($"Attempting to get session for session ID {sessionId}");
                string serializedSession = await _db.StringGetAsync(CreateSessionKey(sessionId));
                if (string.IsNullOrEmpty(serializedSession))
                {
                    Log.Warn($"No serialized session found for ID {sessionId}");
                    return null;
                }

                Session session = JsonConvert.DeserializeObject<Session>(serializedSession);
                if (session == null)
                {
                    Log.Warn($"Failed to deserialize session for ID {sessionId}");
                    return null;
                }

                Log.Debug($"Successfully retrieved and processed session for ID {sessionId}");
                return session;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to get session for session ID {sessionId}", ex);
                return null;
            }
        });
        
        
    }
    
    public async Task<Session> GetSessionByAccountId(long accountId)
    {
        
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            try
            {
                if (!await IsRedisHealthy())
                {
                    Log.Warn($"--------- Redis is unhealthy. Unable to get session for account ID {accountId}");
                }
                Log.Debug($"Attempting to get session for account ID {accountId}");
                string sessionId = await _db.StringGetAsync(CreateAccountIdMappingKey(accountId));
                if (string.IsNullOrEmpty(sessionId))
                {
                    Log.Warn($"No session found for account ID {accountId}");
                    return null;
                }

                Session session = await GetSessionById(sessionId);
                //Log.Debug($"Session retrieval for account ID {accountId} {(session != null ? "succeeded" : "failed")}");
                if (session != null )
                {
                    Log.Debug($"Session retrieval for account {session.AccountId} succeeded.");
                }
                else
                {
                    Log.Warn($"Session retrieval for account {session.AccountId} failed.");
                }
                
                return session;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to get session for account ID {accountId}", ex);
                return null;
            }
        });
    }

    private async Task<Session> GetSessionById(string sessionId)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            if (!await IsRedisHealthy())
            {
                Log.Warn($"--------- Redis is unhealthy. Unable to get session for session ID {sessionId}");
            }
            Log.Debug($"Attempting to get session by ID {sessionId}");
            string serializedSession = await _db.StringGetAsync(CreateSessionKey(sessionId));
            if (string.IsNullOrEmpty(serializedSession))
            {
                Log.Warn($"No serialized session found for ID {sessionId}");
                return null;
            }

            Session session = JsonConvert.DeserializeObject<Session>(serializedSession);
            if (session == null)
            {
                Log.Warn($"Failed to deserialize session for ID {sessionId}");
                return null;
            }

            string stateString = await _db.StringGetAsync(CreateSessionStateKey(sessionId));
            if (!string.IsNullOrEmpty(stateString) && Enum.TryParse<SessionState>(stateString, out SessionState state))
            {
                session.State = state;
            }
            else
            {
                Log.Warn($"Session state is null or invalid for session ID {sessionId}");
                session.State = SessionState.InGame;
            }

            Log.Debug($"Successfully retrieved and processed session for ID {sessionId}");
            return session;
        });
    }

    public async Task<bool> Pulse(Session session)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            try
            {
                if (!await IsRedisHealthy())
                {
                    Log.Warn($"--------- Redis is unhealthy. Unable to pulse session of account id: {session.AccountId}");
                }
                Log.Debug($"Attempting to pulse session for account {session.AccountId}");
                ITransaction transaction = _db.CreateTransaction();
                Task<bool> task1 = transaction.KeyExpireAsync(CreateSessionKey(session.Id), Ttl);
                Task<bool> task2 = transaction.KeyExpireAsync(CreateSessionStateKey(session.Id), Ttl);
                Task<bool> task3 = transaction.KeyExpireAsync(CreateAccountIdMappingKey(session.AccountId), Ttl);
                Task<bool> task4 = transaction.KeyExpireAsync(CreateAccountNameMappingKey(session.AccountName), Ttl);

                bool committed = await transaction.ExecuteAsync();
                bool result = committed && await task1 && await task2 && await task3 && await task4;
                
                if (result)
                {
                    Log.Debug($"Session pulse for account {session.AccountId} succeeded.");
                }
                else
                {
                    Log.Warn($"Session pulse for account {session.AccountId} failed.");
                }
                return result;
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to pulse session for account {session.AccountId}", ex);
                return false;
            }
        });
    }

    private static string CreateSessionKey(string sessionId) => $"{SessionPrefix}:{sessionId}";
    private static string CreateSessionStateKey(string sessionId) => $"{SessionStatePrefix}:{sessionId}";
    private static string CreateAccountIdMappingKey(long accountId) => $"{SessionMappingPrefix}:account-id:{accountId}";
    private static string CreateAccountNameMappingKey(string accountName) => $"{SessionMappingPrefix}:account-name:{accountName}";
}