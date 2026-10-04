using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DeadshotModAPI;

internal class SceneLoadWaiter : MonoBehaviour
{
    private const int MaxSpawnSearchAttempts = 300;

    private string _sceneName = string.Empty;
    private AsyncOperation _unloadOperation;
    private GameObject _player;
    private bool _initialized;
    private bool _sceneLoadRequested;
    private bool _playerPlaced;
    private int _spawnSearchAttempts;

    internal void Initialize(string sceneName, AsyncOperation unloadOperation = null)
    {
        try
        {
            Logger.Log($"SceneLoadWaiter initialized with scene: '{sceneName}'");

            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Logger.Error("SceneLoadWaiter received an empty scene name.");
                Destroy(gameObject);
                return;
            }

            _sceneName = sceneName;
            _unloadOperation = unloadOperation;
        }
        catch (Exception ex)
        {
            Logger.Error($"SceneLoadWaiter.Initialize failed: {ex}");
        }
    }

    internal void Update()
    {
        try
        {
            if (string.IsNullOrEmpty(_sceneName))
            {
                return;
            }

            if (_unloadOperation != null)
            {
                if (!_unloadOperation.isDone)
                {
                    return;
                }

                _unloadOperation = null;
            }

            if (!_initialized)
            {
                InitializePlayer();
            }

            Scene customScene = GetCustomScene();

            if (!customScene.IsValid() || !customScene.isLoaded)
            {
                if (!_sceneLoadRequested)
                {
                    _sceneLoadRequested = true;
                    Logger.Log($"Loading custom scene: {_sceneName}");

                    try
                    {
                        UnityEngine.SceneManagement.SceneManager.LoadScene(_sceneName, LoadSceneMode.Additive);
                    }
                    catch (Exception ex)
                    {
                        _sceneLoadRequested = false;
                        Logger.Error($"Failed to load custom scene '{_sceneName}': {ex}");
                        Destroy(gameObject);
                        return;
                    }
                }

                return;
            }

            if (_player == null)
            {
                _initialized = false;
                return;
            }

            GameObject spawnPoint = FindCustomPlayerSpawn(customScene);

            if (spawnPoint == null)
            {
                _spawnSearchAttempts++;
                if (_spawnSearchAttempts > MaxSpawnSearchAttempts)
                {
                    Logger.Error($"Could not find CustomPlayerSpawn in scene '{_sceneName}'.");
                    Destroy(gameObject);
                }

                return;
            }

            CharacterController controller = _player.GetComponent<CharacterController>();

            if (controller == null)
            {
                Logger.Error("Failed to find player controller.");
                Destroy(gameObject);
                return;
            }

            DisableAllCameras();
            DisableMenuAndSceneRenderers();

            controller.enabled = false;

            try
            {
                _player.transform.SetPositionAndRotation(
                    spawnPoint.transform.position,
                    spawnPoint.transform.rotation
                );
            }
            finally
            {
                controller.enabled = true;
            }

            _playerPlaced = true;
            Logger.Log($"Moved player to CustomPlayerSpawn: {spawnPoint.transform.position}");
            Destroy(gameObject);
        }
        catch (Exception ex)
        {
            Logger.Error($"SceneLoadWaiter.Update failed: {ex}");
        }
    }

    internal void OnDestroy()
    {
        SceneManager.IsSceneLoading = false;

        if (!_playerPlaced)
        {
            RestoreMenuAndSceneRenderers();
        }
    }

    private void InitializePlayer()
    {
        try
        {
            if (!GetPlayer())
            {
                return;
            }

            _initialized = true;
        }
        catch (Exception ex)
        {
            Logger.Error($"SceneLoadWaiter.InitializePlayer failed: {ex}");
        }
    }

    private bool GetPlayer()
    {
        Deadshot.GameManager gameManager = Deadshot.GameManager.INSTANCE;

        if (gameManager == null || gameManager.PlayerManager == null)
        {
            return false;
        }

        _player = gameManager.PlayerManager.gameObject;

        if (_player == null)
        {
            return false;
        }

        Logger.Log($"Found player: {_player.name}");
        return true;
    }

    private bool DisableAllCameras()
    {
        Transform playerCameraTransform = _player.transform.Find("Head/MainCamera");

        if (playerCameraTransform == null)
        {
            return false;
        }

        Camera playerCamera = playerCameraTransform.GetComponent<Camera>();

        if (playerCamera == null)
        {
            return false;
        }

        foreach (Camera camera in FindObjectsByType<Camera>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None))
        {
            if (camera == null)
            {
                continue;
            }

            camera.enabled = camera == playerCamera;
        }

        return true;
    }

    private void DisableMenuAndSceneRenderers()
    {
        foreach (Canvas canvas in FindObjectsByType<Canvas>(
                    FindObjectsInactive.Include,
                    FindObjectsSortMode.None))
        {
            if (canvas == null)
            {
                continue;
            }

            if (canvas.gameObject.scene.name == "MainMenu")
            {
                canvas.enabled = false;
            }
        }

        foreach (Renderer renderer in FindObjectsByType<Renderer>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None))
        {
            if (renderer == null)
            {
                continue;
            }

            if (renderer.gameObject.scene.name != SceneManager.BaseGameplaySceneName)
            {
                continue;
            }

            if (renderer.transform.IsChildOf(_player.transform))
            {
                continue;
            }

            renderer.enabled = false;
        }
    }

    private void RestoreMenuAndSceneRenderers()
    {
        foreach (Renderer renderer in FindObjectsByType<Renderer>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None))
        {
            if (renderer == null)
            {
                continue;
            }

            if (renderer.gameObject.scene.name == SceneManager.BaseGameplaySceneName)
            {
                renderer.enabled = true;
            }
        }
    }

    private Scene GetCustomScene()
    {
        for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
        {
            Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);

            if (scene.name == _sceneName)
            {
                return scene;
            }
        }

        return default;
    }

    private static GameObject FindCustomPlayerSpawn(Scene customScene)
    {
        try
        {
            if (!customScene.IsValid() || !customScene.isLoaded)
            {
                return null;
            }

            foreach (GameObject root in customScene.GetRootGameObjects())
            {
                if (root == null)
                {
                    continue;
                }

                GameObject spawn = FindChildRecursive(root.transform, "CustomPlayerSpawn");

                if (spawn != null)
                {
                    return spawn;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"SceneLoadWaiter.FindCustomPlayerSpawn failed: {ex}");
        }

        return null;
    }

    private static GameObject FindChildRecursive(Transform parent, string name)
    {
        if (parent == null)
        {
            return null;
        }

        if (parent.name == name)
        {
            return parent.gameObject;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);

            if (child == null)
            {
                continue;
            }

            GameObject result = FindChildRecursive(child, name);

            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
}