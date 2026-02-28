using Beta.DI;
using Beta.Scenes;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Beta.Actors;
using Beta.Entities;
using Beta.Logging;
using System;
using Beta.Services.SaveGames.SaveGameDatas;
using BDSM.Runtime;
using BDSM.Runtime.SaveGames;
using System.Threading.Tasks;
using System.Globalization;
using Beta.BDSM;
using Microsoft.Xna.Framework.Graphics;
using Beta.Extensions.Models;
using Google.Protobuf;
using Beta.GameStates;
using Beta.ContentTools;

namespace Beta.Services.SaveGames;

public class SaveGameService
{
    private readonly Driver _bdsmDriver;
    private readonly BDSMAdapter _bdsmAdapter;
    private readonly GameStateManager _gameStateManager;
    private readonly EntityManager _entityManager;
    private readonly SceneManager _sceneManager;
    private readonly ILogger _logger;
    private readonly SaveGameCache _saveGameCache;
    private readonly GraphicsDeviceManager _graphics;
    private readonly IContentPathProvider _contentPathProvider;
    private readonly string _saveGamePath;
    private readonly Dictionary<string, object> _fileLocks = [];

    public SaveGameService()
    {
        _bdsmDriver = DependencyContainer.Instance.Get<Driver>();
        _bdsmAdapter = DependencyContainer.Instance.Get<BDSMAdapter>();
        _gameStateManager = DependencyContainer.Instance.Get<GameStateManager>();
        _entityManager = DependencyContainer.Instance.Get<EntityManager>();
        _sceneManager = DependencyContainer.Instance.Get<SceneManager>();
        _logger = DependencyContainer.Instance.Get<ILogger>();
        _saveGameCache = DependencyContainer.Instance.Get<SaveGameCache>();
        _graphics = DependencyContainer.Instance.Get<GraphicsDeviceManager>();
        _contentPathProvider = DependencyContainer.Instance.Get<IContentPathProvider>();
        _saveGamePath = _contentPathProvider.ProvideSaveGamePath();
    }

    public void Save(string saveGameName)
    {
        if (!Directory.Exists(_saveGamePath))
        {
            Directory.CreateDirectory(_saveGamePath);
        }

        var saveData = _bdsmDriver.GetBDSMSaveData();
        var actorsData = saveData.Actors.ToDictionary(
                a => a.Key,
                a => new ActorSaveData
                {
                    Fields = a.Value.Fields,
                    State = a.Value.State,
                    Costume = a.Value.Costume,
                    // TODO: Can't make Parent nullable for some reason :c
                    Parent = _entityManager.Get<Actor>(a.Key).Parent?.DeclName ?? "",
                });

        var scenesData = saveData.Scenes.ToDictionary(
                s => s.Key,
                s => new SceneSaveData
                {
                    Fields = s.Value.Fields,
                });

        foreach (var sceneData in scenesData)
        {
            var entities = _entityManager
                .GetOnScene(_sceneManager.GetScene(sceneData.Key))
                .Select(e =>
                    new SceneEntitySaveData
                    {
                        EntityName = e.DeclName,
                        Position = e.Position.ToSurrogate(),
                    }
                ).ToList();

            foreach (var entity in entities)
            {
                sceneData.Value.Entities.Add(entity);
            }
        }

        if (_entityManager.Player is null)
        {
            throw new InvalidOperationException("No player defined.");
        }
        if (_entityManager.Player.Scene is null)
        {
            throw new InvalidOperationException("Player not on scene.");
        }
        var singlePlayerData = new PlayerSaveData
        {
            CurrentScene = _entityManager.Player.Scene.Name,
            Position = _entityManager.Player.Position.ToSurrogate(),
        };
        var inventoryItems = _entityManager.Player.Inventory.Items.Select(i => i.DeclName).ToList();
        foreach (var item in inventoryItems)
        {
            singlePlayerData.InventoryItems.Add(item);
        };

        var playersData = new Dictionary<string, PlayerSaveData>
        {
            [_entityManager.Player.DeclName] = singlePlayerData
        };

        // Take screenshot... 
        // var screenshot = TakeScreenShot();

        var saveGame = new SaveGameData
        {
            Fields = saveData.Fields,
            Time = DateTime.Now.ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture),
            CurrentScene = _entityManager.Player.Scene.Name,
        };
        foreach (var actorData in actorsData)
        {
            saveGame.Actors.Add(actorData.Key, actorData.Value);
        }
        foreach (var sceneData in scenesData)
        {
            saveGame.Scenes.Add(sceneData.Key, sceneData.Value);
        }
        foreach (var playerData in playersData)
        {
            saveGame.Players.Add(playerData.Key, playerData.Value);
        }

        var saveGamePath = GetSaveGamePath(saveGameName);
        var saveTask = Task.Run(() =>
        {
            lock (GetFileLock(saveGameName))
            {
                using var file = File.Create(saveGamePath);
                saveGame.WriteTo(file);
            }
        });

        _saveGameCache.CacheSaveGame(saveGame, saveGameName);
    }

    // TODO: This thing is not playing nice with loading afterwards.
    private byte[] TakeScreenShot()
    {
        var backBuffer = new int[1920 * 1080];
        _graphics.GraphicsDevice.GetBackBufferData(backBuffer);
        var texture = new Texture2D(
            _graphics.GraphicsDevice,
            1920, 1080,
            false,
            _graphics.GraphicsDevice.PresentationParameters.BackBufferFormat);
        texture.SetData(backBuffer);
        using var memStream = new MemoryStream();
        texture.SaveAsPng(memStream, 480, 81);

        var screenshot = new Span<byte>();
        memStream.Read(screenshot);

        return screenshot.ToArray();
    }

    public bool TryReadSaveFile(string saveGameName, out SaveGameData? data)
    {
        var saveGamePath = GetSaveGamePath(saveGameName);
        try
        {
            lock (GetFileLock(saveGameName))
            {
                using var file = File.OpenRead(saveGamePath);
                data = SaveGameData.Parser.ParseFrom(file);
                return true;
            }
        }
        catch (Exception e)
        {
            _logger.Error($"Exception while reading save game `{saveGameName}`: {e}");
            data = null;
            return false;
        }
    }

    public SaveGameData? FetchFromCache(string saveName)
    {
        if (!_saveGameCache.IsLoaded)
        {
            return null;
        }
        return _saveGameCache.GetSaveGame(saveName);
    }

    private bool TryLoad(SaveGameData data, out LoadResult loadResult)
    {
        // Need to cleanup everything first.
        _entityManager.RemoveAllFromScenes();

        var actorsData = data.Actors.ToDictionary(
            a => a.Key,
            a => new BDSMActorSaveData
            {
                Fields = a.Value.Fields,
                State = a.Value.State,
                Costume = a.Value.Costume,
            });

        var scenesData = data.Scenes.ToDictionary(
            a => a.Key,
            a => new BDSMSceneSaveData
            {
                Fields = a.Value.Fields,
            });

        var bdsmSaveData = new BDSMSaveData
        {
            Fields = data.Fields,
            Actors = actorsData,
            Scenes = scenesData
        };

        _bdsmDriver.SetBDSMSaveData(bdsmSaveData);

        //////////////////////////////////////////////////////////
        // Scenes
        foreach (var d in data.Scenes)
        {
            foreach (var savedEntity in d.Value.Entities)
            {
                var sceneEntity = _entityManager.Get<Entity>(savedEntity.EntityName);

                sceneEntity.Position = savedEntity.Position.ToVector2();
                sceneEntity.Scene = _sceneManager.GetScene(d.Key);
            }
        }

        //////////////////////////////////////////////////////////
        // Actors
        foreach (var d in data.Actors)
        {
            var actor = _entityManager.Get<Actor>(d.Key);
            actor.ForceState(new ActorState(d.Value.State));
            actor.SetCostume(d.Value.Costume);
            if (string.IsNullOrEmpty(d.Value.Parent))
            {
                actor.Parent = null;
            }
            else
            {
                actor.Parent = _entityManager.Get<Actor>(d.Value.Parent);
            }
        }

        //////////////////////////////////////////////////////////
        // Player
        var playerData = data.Players.First(); // Only one player for now.
        var player = _entityManager.Get<Actor>(playerData.Key);

        var currentScene = _sceneManager.GetScene(playerData.Value.CurrentScene);

        player.Position = playerData.Value.Position.ToVector2();
        player.Scene = currentScene;
        player.Inventory.Items.Clear();
        foreach (var itemName in playerData.Value.InventoryItems)
        {
            player.Inventory.AddItem(_entityManager.Get<Actor>(itemName));
        }
        var region = player.Scene.Regions.FirstOrDefault(r => r.Polygon.Contains(player.Position));
        player.Region = region;
        if (player.Region is not null)
        {
            _bdsmDriver.Interpreter.SetSceneRegionEntered(
                player.Scene.Name,
                player.Region.Name,
                true);
        }

        loadResult = new LoadResult
        {
            Player = player,
            CurrentScene = currentScene
        };
        return true;
    }

    public bool TryLoadGameFromSaveName(string saveGameName)
    {
        if (!_saveGameCache.IsLoaded)
        {
            return false;
        }

        var data = _saveGameCache.GetSaveGame(saveGameName);
        if (data is null)
        {
            return false;
        }

        _bdsmAdapter.ReinitGame();

        var isSuccess = TryLoad(data, out var loadResult);
        if (!isSuccess)
        {
            // TODO: Probably needs better reporting.
            _logger.Error("Failed to load game.");
            return false;
        }

        _entityManager.Player = loadResult.Player;

        _sceneManager.SetSceneNoEntityReset(loadResult.CurrentScene.Name);

        if (_sceneManager.CurrentScene is null)
        {
            throw new InvalidOperationException("No scene.");
        }
        _bdsmDriver.Interpreter.SetSceneVariable(_sceneManager.CurrentScene.Name);
        _bdsmDriver.Run("posthook()");

        _gameStateManager.RequestStatePlaying();

        return true;
    }

    public bool LoadAutosave()
    {
        var saveGameName = "autosave";
        return TryLoadGameFromSaveName(saveGameName);
    }
    public bool LoadGameFromSlot(int slot)
    {
        var saveGameName = $"save_{slot}";
        return TryLoadGameFromSaveName(saveGameName);
    }

    public void SaveGameToSlot(int slot)
    {
        var saveGameName = $"save_{slot}";
        Save(saveGameName);

        ValidateSaveGameAndRetryOnFail(saveGameName);
    }

    public void AutosaveGame()
    {
        var saveGameName = "autosave";
        Save(saveGameName);

        ValidateSaveGameAndRetryOnFail(saveGameName);
    }

    private void ValidateSaveGameAndRetryOnFail(string saveGameName)
    {
        Task.Run(() =>
        {
            if (!TryReadSaveFile(saveGameName, out var _))
            {
                // Retry one time... Should be something more clever probably.
                // Need to also report this.
                Save(saveGameName);
            }
        });
    }

    private string GetSaveGamePath(string saveGameName)
    {
        return Path.Combine(_saveGamePath, $"{saveGameName}.dat");
    }

    private object GetFileLock(string fileName)
    {
        if (_fileLocks.TryGetValue(fileName, out var fileLock))
        {
            return fileLock;
        }
        var newFileLock = new object();
        _fileLocks[fileName] = newFileLock;
        return newFileLock;
    }
}