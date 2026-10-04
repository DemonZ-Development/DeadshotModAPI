# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
## [1.1.1-dev-alpha] - 2026-10-02

### Added

* `SceneLoadWaiter.GetPlayer()` — extracted helper that resolves the player `GameObject` from `Deadshot.GameManager`.
* `SceneLoadWaiter.DisableAllCameras()` — extracted helper that disables all cameras except the player's `Head/MainCamera`.
* `SceneLoadWaiter.DisableMenuAndSceneRenderers()` — extracted helper that hides `MainMenu` canvases and `C1L2` scene renderers, excluding the player.
* `SceneLoadWaiter.GetCustomScene()` — extracted helper that scans loaded scenes by name and returns the matching `Scene`.
* `SceneLoadWaiter.FindCustomPlayerSpawn(Scene)` — updated to accept the already-resolved `Scene` instead of re-scanning for it.
* Added null guards and restored deep recursive traversal in `FindChildRecursive` to support nested hierarchy spawn points.
* Player `CharacterController` is now restored in a `finally` block, ensuring it is re-enabled even if player placement throws.
* `SceneManager.BaseGameplaySceneName` constant to avoid hardcoded scene name strings.
* `AsyncOperation` tracking for asynchronous scene unloads to prevent reload race conditions.

### Changed

* `SceneLoadWaiter.InitializePlayer()` refactored to delegate player, camera, and renderer setup to dedicated helper methods.
* Corrected the renderer-filtering scene name from `C1L1` to `C1L2`.
* `SceneManager.LoadScene()` cleans up existing waiters and passes any pending unload operation to the new waiter.
* `SceneLoadWaiter` destroys its `GameObject` upon placement completion or spawn search timeout.
* `EventManager.Update()` converted from a static method to an instance lifecycle method on `MonoBehaviour`.

### Fixed

* Resolved persistent `SceneLoadWaiter` memory leak and CPU overhead after scene load.
* Fixed broken child traversal in `FindChildRecursive` failing on nested spawn points.
* Fixed race condition where reloading an already loaded scene attempted placement before unloader completed.
* Fixed `EventManager.Update()` not being invoked by Unity's engine loop.
* Removed stray blank lines from `EventManager.LevelPlaytimeEvent()`.

## [1.1.0-dev-alpha] - 2026-10-02

### Added
- `SceneLoadWaiter` MonoBehaviour for additive custom-scene loading while preserving the existing gameplay scene and player.
- `SceneManager.LoadScene(string)` replacing the old `Load()` method — unloads an already-loaded scene before re-loading it, and bootstraps `C1L2` if it is not yet loaded.
- `AssetBundleManager.GetPlayerObject()` helper to retrieve the player `GameObject` from `Deadshot.GameManager`.
- `AssetBundleManager.SpawnPlayerObject()` helper to instantiate the player object.
- `LevelCompleteScreen.SetSecret(string)` to write to `_menu.secretText`.
- `Plugin.cs` now registers `SceneLoadWaiter` as a component on startup.
- `UnityEngine.UIModule` and `UnityEngine.PhysicsModule` references added to `DeadshotModAPI.csproj`.
- `.editorconfig` with comprehensive C# and project-wide style rules, including test-project overrides.
- `EnforceCodeStyleInBuild` enabled in `DeadshotModAPI.csproj`.
- `UniverseLib` explicit project reference added to `DeadshotModAPI.csproj`.

### Changed
- `SceneManager.LoadModsBundle()` is now `public` and delegates to `AssetBundleManager` instead of using a raw `AssetBundle` field.
- `AssetBundleManager` bundle root path changed from `BepInEx/mods` to `BepInEx/plugins/mods`.
- `AssetBundleManager.LoadAssetBundles()` now returns an empty `List<>` instead of `null` on individual bundle load failure.
- `Input.CheckKeys()` refactored to filter with `Where(IsPressed)` before iterating, removing the inner `if` branch.
- `Input.CheckKeys()` loop variables changed from `var` to explicit `Key[]` / `Key` types.
- `EventManager` level-time comparison changed from `==` to `< 0.001f` float epsilon check.
- `LevelCompleteScreen.SetTime()` simplified — redundant string interpolation removed.
- `Plugin.cs` `_harmony` static field removed; `Harmony` instance is now a local variable.
- `ModLoader.Start()` calls `SceneManager.LoadModsBundle()` on startup.
- `ModLoader` null-guard braces added for `types == null` and `type == null` checks.
- `SceneManager` using directives re-ordered and `Deadshot.Player` namespace added.

### Fixed
- Fixed `catch(Exception` missing space — normalised to `catch (Exception` throughout `EventManager.cs`.
- Fixed stray blank line inside `EventManager.LevelPlaytimeEvent()`.

## [1.0.0-dev-alpha] - 2026-09-28

### Added
- `EventManager.cs` for events.
- `LevelCompleteScreen.cs` for modifying `Deadshot.UI.Menus.LevelCompletedMenu`.
- `Harmony v2.4.2` for patching game instances.

### Changed
- Updated `Plugin.cs` version to `v1.0.0-dev-alpha`.
- Updated `Plugin.Load()` to add `EventManager` as component.
- Added null checks and `try/catch` exception handling to EventManager.

### Fixed
- Fixed runtime loading for `GameAssembly.dll` in `DeadshotModAPI.Tests.csproj`.
- Fixed git merge conflict errors with `origin/dev`.
- Fixed `LevelCompletedScreen.SetTime()` having a hardcoded text.
- Fixed spelling mistake in `Plugin.cs`

## [1.1.0] - 2026-09-28

### Added
- `Input.RemoveKeyPressed(Key, Action)` to unregister key callbacks.
- `examples/ExampleMod/` reference project featuring Subaka's `SpeedrunMod`.
- `DeadshotModAPI.Tests/` automated test suite covering mod discovery, reflection, and edge cases.
- `CONTRIBUTING.md` and `.github/pull_request_template.md`.

### Changed
- Updated `Plugin.cs` version to `1.1.0`.
- Converted all assembly references in `DeadshotModAPI.csproj` to relative paths (`lib\*.dll`).
- Renamed `Class1.cs` to `Plugin.cs`.
- Renamed `Logging/logger.cs` to `Logging/Logger.cs`.
- Upgraded GitHub Actions CI workflow to `actions/checkout@v4`.
- Rewrote `README.md` with clear setup instructions and API reference.

### Fixed
- Fixed hardcoded machine paths (`C:\Code\...`) and missing path separators in `DeadshotModAPI.csproj`.
- Added defensive null checks and `try/catch` isolation in `ModLoader` so corrupted or invalid mod DLLs log errors without crashing the game or stopping other mods.
- Added input validation to `SceneManager.Load` for null, whitespace, and negative indices.
- Added null guards in `GameManager.RestartLevel` before accessing game singletons and save data dictionaries.
- Fixed key collection mutation issue in `Input.CheckKeys` by snapshotting keys before dispatching callbacks.

## [1.0.0] - 2026-09-27

### Added
- Initial modding runtime for Deadshot on BepInEx 6 IL2CPP and .NET 6.
- `IDeadshotMod` interface.
- Dynamic mod loader scanning `BepInEx/mods/`.
- Keyboard input listening via Unity Input System.
- Level restart helper in `GameManager`.
- Scene loading helpers in `SceneManager`.
- Console logging via `Logger`.
