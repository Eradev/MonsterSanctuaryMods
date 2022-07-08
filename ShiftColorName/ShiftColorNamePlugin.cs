using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace eradev.monstersanctuary.ShiftColorName
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class ShiftColorNamePlugin : BaseUnityPlugin
    {
        private static ManualLogSource _log;

        private void Awake()
        {
            _log = Logger;

            new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        [HarmonyPatch(typeof(MonsterSummary), "SetMonster")]
        private class MonsterSummarySetMonsterPatch
        {
            private static void Postfix(ref MonsterSummary __instance)
            {
                var monster = __instance.Monster;
                _log.LogDebug($"MonsterSummary.SetMonster :: {monster.GetName()} ({monster.Shift})");
                _log.LogDebug($"    Before: {__instance.Name.color.ToHtmlRGBA()}");

                var shiftColor = monster.Shift switch
                {
                    EShift.Normal => Color.gray,
                    EShift.Light => GameDefines.ColorLightShift,
                    EShift.Dark => GameDefines.ColorDarkShift,
                    _ => throw new ArgumentOutOfRangeException()
                };

                __instance.Name.color = shiftColor;

                ColorTweenOverwriter.Add(__instance.Name.gameObject);

                _log.LogDebug($"    After: {__instance.Name.color.ToHtmlRGBA()}");
            }
        }

        [HarmonyPatch(typeof(ColorTween), "EndTween", typeof(bool), typeof(bool))]
        private class ColorTweenEndTweenPatch
        {
            private static void Prefix(
                // ReSharper disable once InconsistentNaming
                ref ColorTween __instance,
                ref bool resetColor)
            {
                var text = __instance.gameObject.GetComponent<tk2dTextMesh>();

                if (text != null &&
                    __instance.gameObject.GetComponentInParent<MonsterArmyMenu>() != null ||
                        __instance.gameObject.GetComponentInParent<MonsterSummary>() != null)
                {
                    resetColor = true;
                }
            }
        }

        [HarmonyPatch(typeof(MonsterArmyMenu), "ShowDonateMonsterMenuItem")]
        private class MonsterArmyMenuShowDonateMonsterMenuItemPatch
        {
            private static void Postfix(
                IMenuListDisplayable displayable,
                MenuListItem menuItem)
            {
                var monster = (Monster)displayable;

                _log.LogDebug($"MonsterArmyMenu.ShowDonateMonsterMenuItem :: {monster.GetName()} ({monster.Shift})");
                _log.LogDebug($"    Before: {menuItem.TextColorOverride.ToHtmlRGBA()}");

                menuItem.TextColorOverride = monster.Shift switch
                {
                    EShift.Normal => menuItem.TextColorOverride,
                    EShift.Light => GameDefines.ColorLightShift,
                    EShift.Dark => GameDefines.ColorDarkShift,
                    _ => throw new ArgumentOutOfRangeException()
                };

                _log.LogDebug($"    After: {menuItem.TextColorOverride.ToHtmlRGBA()}");
            }
        }
    }
}
