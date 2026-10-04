using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DeadshotModAPI;

/// <summary>
/// Scene loading and asset bundle management for Deadshot mods.
/// </summary>
public static class SceneManager
{
    internal const string BaseGameplaySceneName = "C1L2";

    internal static bool IsSceneLoading { get; set; }

    /// <summary>
    /// Loads a scene while keeping Deadshot's gameplay scene loaded
    /// so the existing player and gameplay systems remain available.
    /// </summary>
    /// <param name="sceneName">The name of the scene to load.</param>
    public static void LoadScene(string sceneName)
    {
        Logger.Log($"LoadScene called with: '{sceneName}'");

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Logger.Error("LoadScene received an empty scene name.");
            return;
        }

        if (string.Equals(sceneName, BaseGameplaySceneName, StringComparison.OrdinalIgnoreCase))
        {
            Logger.Error($"Cannot load '{BaseGameplaySceneName}' as a custom scene; it is the base gameplay scene.");
            return;
        }

        if (IsSceneLoading)
        {
            Logger.Log($"Scene load already in progress; ignoring duplicate request for '{sceneName}'.");
            return;
        }

        try
        {
            IsSceneLoading = true;

            AsyncOperation unloadOperation = null;
            Scene existingScene = default;

            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);

                if (scene.name == sceneName)
                {
                    existingScene = scene;
                    break;
                }
            }

            if (existingScene.IsValid() && existingScene.isLoaded)
            {
                Logger.Log($"Scene '{sceneName}' is already loaded. Unloading before reloading.");
                unloadOperation = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(existingScene);
            }

            bool baseGameplaySceneLoaded = false;

            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);

                if (scene.name == BaseGameplaySceneName && scene.isLoaded)
                {
                    baseGameplaySceneLoaded = true;
                    break;
                }
            }

            if (!baseGameplaySceneLoaded)
            {
                Logger.Log($"Loading Deadshot gameplay scene: {BaseGameplaySceneName}");

                UnityEngine.SceneManagement.SceneManager.LoadScene(BaseGameplaySceneName, LoadSceneMode.Additive);
            }

            var waiterObject = new GameObject("DeadshotModAPI_SceneLoadWaiter");
            UnityEngine.Object.DontDestroyOnLoad(waiterObject);
            SceneLoadWaiter waiter = waiterObject.AddComponent<SceneLoadWaiter>();
            waiter.Initialize(sceneName, unloadOperation);
        }
        catch (Exception ex)
        {
            IsSceneLoading = false;
            Logger.Error($"Failed to load scene '{sceneName}': {ex}");
        }
    }

    /// <summary>
    /// Loads the Deadshot Mod API asset bundles from
    /// BepInEx/mods/DeadshotModAPI.
    /// </summary>
    internal static void LoadModsBundle()
    {
        try
        {
            AssetBundleManager bundleManager = new();

            List<string> bundlesFiles = new()
            {
                "DeadshotModApi/deadshotmodapi",
                "DeadshotModApi/deadshotapi_assets"
            };

            List<UniverseLib.AssetBundle> bundles = bundleManager.LoadAssetBundles(bundlesFiles);

            if (bundles == null || bundles.Count == 0)
            {
                Logger.Error("Failed to load asset bundles.");
                return;
            }

            Logger.Info($"Loaded {bundles.Count} asset bundles.");
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to load Mods Menu bundle: {ex}");
        }
    }
}