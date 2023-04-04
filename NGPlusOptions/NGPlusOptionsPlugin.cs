using System.Threading.Tasks;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace eradev.monstersanctuary.NGPlusOptions
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [UsedImplicitly]
    public class NGPlusOptionsPlugin : BaseUnityPlugin
    {
        private static SaveGameMenu _saveGameMenu;
        private static int _selectedDifficultyIndex;
        private static bool _ngPlusOptionsDone;

        // ReSharper disable once NotAccessedField.Local
        private static ManualLogSource _log;

        [UsedImplicitly]
        private void Awake()
        {
            _log = Logger;

            new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        private static void AskUnshiftMonsters()
        {
            PopupController.Instance.ShowRequest(
                Utils.LOCA("NG+ Options"),
                Utils.LOCA("Unshift all monsters?"),
                ConfirmUnshiftMonsters,
                AskSellEquipment,
                true
            );
        }

        private static void ConfirmUnshiftMonsters()
        {
            PlayerController.Instance.Monsters.AllMonster.ForEach(x => x.SetShift(EShift.Normal));

            Task.Run(() =>
            {
                PopupController.Instance.ShowMessage(
                    Utils.LOCA("NG+ Options"),
                    Utils.LOCA("All monsters have been unshifted."),
                    AskSellEquipment);
            });
        }

        private static void AskSellEquipment()
        {
            PopupController.Instance.ShowRequest(
                Utils.LOCA("NG+ Options"),
                Utils.LOCA("Sell all weapons and accessories?"),
                ConfirmSellEquipment,
                CompleteNGPlusOptions,
                true
            );
        }

        private static void ConfirmSellEquipment()
        {
            PlayerController.Instance.Monsters.AllMonster.ForEach(x => x.Equipment.UnequipAll());

            PlayerController.Instance.Inventory.Weapons.ForEach(x =>
                PlayerController.Instance.Gold += Mathf.RoundToInt(x.Item.Price * 0.3f) * x.Quantity);
            PlayerController.Instance.Inventory.Accessories.ForEach(x =>
                PlayerController.Instance.Gold += Mathf.RoundToInt(x.Item.Price * 0.3f) * x.Quantity);

            PlayerController.Instance.Inventory.Weapons.Clear();
            PlayerController.Instance.Inventory.Accessories.Clear();

            Task.Run(() =>
            {
                PopupController.Instance.ShowMessage(
                    Utils.LOCA("NG+ Options"),
                    Utils.LOCA("All weapons and accessories have been sold."),
                    CompleteNGPlusOptions
                );
            });
        }

        private static void CompleteNGPlusOptions()
        {
            _ngPlusOptionsDone = true;

            AccessTools.Method(typeof(SaveGameMenu), "DifficultyChosen").Invoke(_saveGameMenu, new object[]
            {
                _selectedDifficultyIndex
            });
        }

        [HarmonyPatch(typeof(SaveGameMenu), "DifficultyChosen")]
        private class GameControllerLoadStartingAreaPatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref SaveGameMenu __instance, int index)
            {
                if (!PlayerController.Instance.NewGamePlus || _ngPlusOptionsDone)
                {
                    _ngPlusOptionsDone = false;

                    return true;
                }

                _saveGameMenu = __instance;
                _selectedDifficultyIndex = index;

                AskUnshiftMonsters();

                return false;
            }
        }
    }
}
