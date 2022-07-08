using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace eradev.monstersanctuary.LevelCaps
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class LevelCapsPlugin : BaseUnityPlugin
    {
        private static ManualLogSource _log;

        private const int MaxLevelSelfDefault = 42;
        private const int MaxLevelEnemyDefault = 42;

        private static ConfigEntry<int> _maxLevelSelf;
        private static ConfigEntry<int> _maxLevelEnemy;

        private void Awake()
        {
            _log = Logger;

            _maxLevelSelf = Config.Bind("Config", "Level cap self", MaxLevelSelfDefault, "Level cap for your monsters");
            _maxLevelEnemy = Config.Bind("Config", "Level cap enemies", MaxLevelEnemyDefault, "Level cap for enemies");

            new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        [HarmonyPatch(typeof(PlayerController), "CurrentSpawnLevel", MethodType.Getter)]
        private class PlayerControllerCurrentSpawnLevelPatch
        {
            private static void Postfix(ref int __result)
            {
                __result = Math.Min(__result, _maxLevelEnemy.Value);
            }
        }

        [HarmonyPatch(typeof(GameController), "Awake")]
        private class GameControllerAwakePatch
        {
            private static void Postfix()
            {
                GameController.LevelCap = _maxLevelSelf.Value;

                _log.LogDebug($"Level cap set to {GameController.LevelCap}.");
            }
        }
    }
}
