using Beta.DI;
using System.Collections.Generic;
using System.IO;
using Beta.Logging;
using System;
using Beta.Services.SaveGames.SaveGameDatas;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Beta.ContentTools;

namespace Beta.Services.SaveGames;

public class SaveGameCache
{
    public bool IsLoaded { get; private set; }

    private readonly ConcurrentDictionary<string, SaveGameData> _saveGames = [];
    private readonly ILogger _logger;
    private readonly string _saveGamePath;

    public SaveGameCache()
    {
        _logger = DependencyContainer.Instance.Get<ILogger>();
        _saveGamePath = DependencyContainer.Instance.Get<IContentPathProvider>()
            .ProvideSaveGamePath();
    }

    public void PreloadAllSaveGames()
    {
        if (!Directory.Exists(_saveGamePath))
        {
            IsLoaded = true;
            return;
        }

        List<Task> tasks = [];

        var saves = Directory.GetFiles(_saveGamePath);
        foreach (var saveGamePath in saves)
        {
            _logger.Debug($"Will read save from `{saveGamePath}`...");
            var saveGameName = Path.GetFileNameWithoutExtension(saveGamePath);
            var task = Task.Run(() =>
            {
                TryReadSaveFile(saveGamePath, out var saveData);
                if (saveData is null)
                {
                    throw new InvalidOperationException($"Failed to read save game at {saveGamePath}");
                }
                _saveGames[saveGameName] = saveData;
                _logger.Debug($"Cached save data from {saveGamePath} for {saveGameName}");
            });
            tasks.Add(task);
        }
        Task.WhenAll(tasks).ContinueWith((t) =>
        {
            IsLoaded = true;
        });
    }

    public void CacheSaveGame(SaveGameData data, string saveGameName)
    {
        if (!IsLoaded)
        {
            throw new InvalidOperationException("Cache was not yet loaded.");
        }
        _saveGames[saveGameName] = data;
    }

    public SaveGameData? GetSaveGame(string saveGameName)
    {
        if (!IsLoaded)
        {
            throw new InvalidOperationException("Cache was not yet loaded.");
        }
        return _saveGames.GetValueOrDefault(saveGameName);
    }

    private bool TryReadSaveFile(string saveGamePath, out SaveGameData? data)
    {
        try
        {
            using var file = File.OpenRead(saveGamePath);
            data = SaveGameData.Parser.ParseFrom(file);
            return true;
        }
        catch (Exception e)
        {
            _logger.Error($"Exception while reading save file `{saveGamePath}`: {e}");
            data = null;
            return false;
        }
    }
}