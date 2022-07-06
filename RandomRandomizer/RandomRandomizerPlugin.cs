using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace eradev.monstersanctuary.RandomRandomizer
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class RandomRandomizerPlugin : BaseUnityPlugin
    {
        private static readonly System.Random Rand = new();
        private static ManualLogSource _log;

        private static List<GameObject> _possibleItemsList;

        private const bool RandomizedMonstersEnabledDefault = true;
        private const string MonstersBlacklistDefault = "228,317,348,361,1879"; // Spectral (228,317,348,361), Bard (1879)

        private static ConfigEntry<bool> _randomizeMonstersEnabled;
        private static ConfigEntry<string> _monstersBlacklist;

        private const bool RandomizedChestsEnabledDefault = true;
        private const float GoldChanceDefault = 0.05f;
        private const int MinGoldDefault = 5;
        private const int MaxGoldDefault = 50;
        private const string ItemsBlacklistDefault = "1792,1793,1794,1795,1796,1797"; // Eternity Flame (1792~1797)

        private static ConfigEntry<bool> _randomizeChestsEnabled;
        private static ConfigEntry<float> _goldChance;
        private static ConfigEntry<int> _minGold;
        private static ConfigEntry<int> _maxGold;
        private static ConfigEntry<string> _itemsBlacklist;

        private void Awake()
        {
            _randomizeMonstersEnabled = Config.Bind("Randomized Monsters", "Enabled", RandomizedMonstersEnabledDefault, "Randomize monsters");
            _monstersBlacklist = Config.Bind("Randomized Monsters", "Blacklist", MonstersBlacklistDefault, "Blacklisted monsters ID");

            _randomizeChestsEnabled = Config.Bind("Randomized Chests", "Enabled", RandomizedChestsEnabledDefault, "Randomize chests");
            _goldChance = Config.Bind("Randomized Chests", "Chance for gold", GoldChanceDefault, "Chance to get gold in chests (0.0 = never, 1.0 = always)");
            _minGold = Config.Bind("Randomized Chests", "Minimum gold", MinGoldDefault, "Minimum value of gold in chests (x100, must be > 0)");
            _maxGold = Config.Bind("Randomized Chests", "Maximum gold", MaxGoldDefault, "Minimum value of gold in chests (x100, must be > 0)");
            _itemsBlacklist = Config.Bind("Randomized Chests", "Blacklist", ItemsBlacklistDefault, "Blacklisted items ID");

            // Ensure minGold is always higher than 0.
            if (_minGold.Value == 0)
            {
                _minGold.Value = MinGoldDefault;

                Logger.LogInfo("The minimum gold value has been reset.");
            }

            // Ensure maxGold is always higher than 0.
            if (_maxGold.Value == 0)
            {
                _maxGold.Value = MaxGoldDefault;

                Logger.LogInfo("The maximum gold value has been reset.");
            }

            // Swap values if incorrect
            if (_maxGold.Value < _minGold.Value)
            {
                (_maxGold.Value, _minGold.Value) = (_minGold.Value, _maxGold.Value);

                Logger.LogInfo("The minimum and maximum gold values have been swapped.");
            }

            _log = Logger;

            new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        [HarmonyPatch(typeof(MonsterEncounter), "Start")]
        private class MonsterEncounterStartPatch
        {
            // ReSharper disable once InconsistentNaming
            private static void Prefix(ref MonsterEncounter __instance)
            {
                if (!_randomizeMonstersEnabled.Value)
                {
                    _log.LogDebug("MonsterRandomizer ignore: Disabled in config");

                    return;
                }

                if (!GameModeManager.Instance.RandomizerMode)
                {
                    _log.LogDebug("MonsterRandomizer ignore: Not in Randomizer mode");

                    return;
                }

                if (!__instance.IsNormalEncounter)
                {
                    _log.LogDebug("MonsterRandomizer ignore: Not a normal encounter");

                    return;
                }

                _log.LogDebug("Randomizing encounter...");
                _log.LogDebug("    Before:");
                foreach(var m in __instance.PredefinedMonsters.Monster)
                {
                    _log.LogDebug($"    * {GameModeManager.Instance.GetReplacementMonster(m.GetComponent<Monster>()).Name}");
                }

                var blacklistedMonsters = _monstersBlacklist.Value.Split(',').Select(int.Parse).ToList();
                var availableMonsters = GameController.Instance.MonsterJournalList
                    .Where(x => !blacklistedMonsters.Contains(x.GetComponent<Monster>().ID))
                    .ToList();

                for (var i = 0; i < 3; i++)
                {
                    __instance.PredefinedMonsters.Monster[i] = availableMonsters[Random.Range(0, availableMonsters.Count)];
                }

                _log.LogDebug("    After:");
                foreach (var m in __instance.PredefinedMonsters.Monster)
                {
                    _log.LogDebug($"    * {GameModeManager.Instance.GetReplacementMonster(m.GetComponent<Monster>()).Name}");
                }
            }
        }

        [HarmonyPatch(typeof(Chest), "OpenChest")]
        private class ChestOpenChestPatch
        {
            // ReSharper disable once InconsistentNaming
            private static void Prefix(ref Chest __instance)
            {
                if (!_randomizeChestsEnabled.Value)
                {
                    _log.LogDebug("ChestRandomizer ignore: Disabled in config");

                    return;
                }

                if (!GameModeManager.Instance.RandomizerMode)
                {
                    _log.LogDebug("ChestRandomizer ignore: Not in Randomizer mode");

                    return;
                }

                if (__instance.BraveryChest)
                {
                    _log.LogDebug("ChestRandomizer ignore: Bravery chest");

                    return;
                }

                if (__instance.Item != null && __instance.Item.GetComponent<KeyItem>() != null)
                {
                    _log.LogDebug("ChestRandomize ignore: Key chest");

                    return;
                }

                if (__instance.Item != null && __instance.Item.GetComponent<UniqueItem>() != null)
                {
                    _log.LogDebug("ChestRandomize ignore: Unique item chest");

                    return;
                }

                if (__instance.ID == 11)
                {
                    _log.LogDebug("ChestRandomize ignore: Raw Hide chest (Quest)");

                    return;
                }

                if (_possibleItemsList == null)
                {
                    var blacklistedItems = _itemsBlacklist.Value.Split(',').Select(int.Parse).ToList();
                    _possibleItemsList = GameController.Instance.WorldData.Referenceables
                        .Where(x => x?.gameObject?.GetComponent<BaseItem>() != null) // All the items
                        .Select(x => x.gameObject)
                        .Where(x => !blacklistedItems.Contains(x.GetComponent<BaseItem>().ID) &&  // Remove blacklisted items
                                    x.GetComponent<KeyItem>() == null && // Remove Keys
                                    x.GetComponent<UniqueItem>() == null && // Remove Unique items (ie. Costumes)
                                    x.GetComponent<Egg>() == null && // Remove Eggs
                                    x.GetComponent<Catalyst>() == null) // Remove Catalysts
                        .ToList();
                }

                _log.LogDebug("Randomizing chest content...");
                _log.LogDebug($"    Chest ID: {__instance.ID}");
                _log.LogDebug($"    Chest name: {__instance.name}");
                _log.LogDebug($"    Before: {(__instance.Gold > 0 ? $"{__instance.Gold} Gold" : __instance.Item?.GetComponent<BaseItem>().GetName())}");

                if (Rand.NextDouble() < _goldChance.Value)
                {
                    __instance.Item = null;
                    __instance.Gold = Rand.Next(_minGold.Value, _maxGold.Value + 1) * 100;
                }
                else
                {
                    __instance.Gold = 0;
                    __instance.Item = _possibleItemsList[Rand.Next(0, _possibleItemsList.Count)];
                    __instance.Quantity = __instance.Item.GetComponent<Equipment>() != null ? 1 : Rand.Next(1, 4);
                }

                _log.LogDebug($"    After: {(__instance.Gold > 0 ? $"{__instance.Gold} Gold" : __instance.Item?.GetComponent<BaseItem>().GetName())}");
            }
        }
    }
}
