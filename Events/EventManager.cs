using System;
using DeadshotGameManager = Deadshot.GameManager;
using Deadshot.UI.Menus;
using HarmonyLib;
using UnityEngine;

namespace DeadshotModAPI;

public class EventManager : MonoBehaviour
{
    internal void Update()
    {
        LevelEvent.LevelPlaytimeEvent();
    }

    public static class LevelEvent
    {
        private static float _lastLevelTime;
        public static event Action<float> LevelTimeChanged;
        public static event Action<LevelCompleteScreen> LevelCompleted;

        internal static void LevelPlaytimeEvent()
        {
            try
            {

                if (DeadshotGameManager.INSTANCE == null)
                {
                    Logger.Error("Cannot restart level: DeadshotGameManager.INSTANCE is null.");
                    return;
                }


                float levelTime = DeadshotGameManager.INSTANCE.CompletionTime;
                if (levelTime - _lastLevelTime < 0.001f)
                {
                    return;
                }

                _lastLevelTime = levelTime;

                LevelTimeChanged?.Invoke(levelTime);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in LevelPlaytimeEvent(): {ex}");
            }
        }

        /// <summary>
        /// Gets the current levels playtime.
        /// </summary>
        /// <returns>The playtime as a float.</returns>
        public static float GetLevelPlaytime()
        {
            try
            {
                if (DeadshotGameManager.INSTANCE == null)
                {
                    Logger.Error($"DeadshotGameManager.INSTANCE is null.");
                }

                return DeadshotGameManager.INSTANCE.CompletionTime;
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in GetLevelPlaytime(): {ex}");
                return 0;
            }
        }

        internal static void LevelCompletedEvent(LevelCompleteScreen menu)
        {
            try
            {
                LevelCompleted?.Invoke(menu);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in LevelCompleted?.Invoke(): {ex}");
            }
        }
    }

    [HarmonyPatch(typeof(LevelCompleteMenu))]
    private static class LevelCompleteMenuPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(LevelCompleteMenu.OnEnable))]
        private static void OnEnable(LevelCompleteMenu __instance)
        {
            try
            {
                if (__instance == null)
                {
                    Logger.Error($"LevelCompleteMenu instance is null.");
                    return;
                }

                var screen = new LevelCompleteScreen(__instance);

                LevelEvent.LevelCompletedEvent(screen);
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in LevelCompleteMenu.OnEnable postfix: {ex}");
            }
        }
    }
}