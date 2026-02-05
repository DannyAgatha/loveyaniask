using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using PhoenixLib.Logging;
using Plugin.Database.DAOs;
using Polly;
using Polly.Retry;
using WingsAPI.Communication;
using WingsAPI.Communication.DbServer.CharacterService;
using WingsAPI.Data.Character;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations.DbServer;

public class CharacterSaveSystem : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(Convert.ToUInt32(Environment.GetEnvironmentVariable("CHARACTER_SAVE_SYSTEM_INTERVAL_SECONDS") ?? "15"));
    private readonly ICharacterService _characterService;
    private readonly IPlayerEntityFactory _playerEntityFactory;
    private readonly ISessionManager _sessionManager;

    public CharacterSaveSystem(ISessionManager sessionManager, IPlayerEntityFactory playerEntityFactory, ICharacterService characterService)
    {
        _sessionManager = sessionManager;
        _playerEntityFactory = playerEntityFactory;
        _characterService = characterService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Log.Debug("[CHARACTER_SAVE_SYSTEM] Started");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessSaves();
            }
            catch (Exception e)
            {
                Log.Error("[CHARACTER_SAVE_SYSTEM]", e);
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private static IEnumerable<List<T>> SplitList<T>(List<T> locations, int nSize = 30)
    {
        for (int i = 0; i < locations.Count; i += nSize)
        {
            yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i));
        }
    }

    private async Task ProcessSaves()
    {
        IReadOnlyList<IClientSession> sessions = _sessionManager.Sessions;
        if (sessions.Count < 1)
        {
            Log.Debug("[CHARACTER_SAVE_SYSTEM] No characters to save");
            return;
        }

        var charactersToSave = new List<CharacterDTO>();
        foreach (IClientSession session in sessions)
        {
            if (session.PlayerEntity == null)
            {
                continue;
            }

            charactersToSave.Add(_playerEntityFactory.CreateCharacterDto(session.PlayerEntity));
        }

        IEnumerable<List<CharacterDTO>> toSave = SplitList(charactersToSave, 50);

        AsyncRetryPolicy policy = Policy.Handle<Exception>().RetryAsync(3, (exception, i1) => Log.Error($"[CHARACTER_SAVE_SYSTEM] Failed to save characters, try {i1.ToString()}. ", exception));

        int i = 0;
        foreach (List<CharacterDTO> chunkToSave in toSave)
        {
            i += chunkToSave.Count;
            Log.Warn($"[CHARACTER_SAVE_SYSTEM] Saving chunk of {i}/{charactersToSave.Count.ToString()}");
            try
            {
                await policy.ExecuteAsync(() => TrySave(chunkToSave));
            }
            catch (Exception e)
            {
                Log.Error($"[CHARACTER_SAVE_SYSTEM] Failed to save chunk of {i}/{charactersToSave.Count.ToString()}", e);
            }
        }
    }

    private async Task TrySave(List<CharacterDTO> chunkToSave)
    {
        DbServerSaveCharactersResponse response = await _characterService.SaveCharacters(new DbServerSaveCharactersRequest
        {
            Characters = chunkToSave
        });

        if (response.RpcResponseType == RpcResponseType.SUCCESS)
        {
            Log.Info($"[CHARACTER_SAVE_SYSTEM] Saved {chunkToSave.Count.ToString()} characters");
            return;
        }
        Log.Warn("[CHARACTER_SAVE_SYSTEM] The saves couldn't be saved, will be saved on next loop");
    }
}