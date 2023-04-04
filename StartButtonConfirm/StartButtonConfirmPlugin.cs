﻿using BepInEx;
using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;

namespace eradev.monstersanctuary.StartButtonConfirm
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class StartButtonConfirmPlugin : BaseUnityPlugin
    {
        [UsedImplicitly]
        private void Awake()
        {
            new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        [HarmonyPatch(typeof(NameMenu), "Update")]
        private class NameMenuUpdatePatch
        {
            [UsedImplicitly]
            private static bool Prefix(ref NameMenu __instance)
            {
                // couldn't get this to work with my controller directly so I use another app
                // to make my controller's start button simulate the PageDown key
                if (Input.GetKeyUp(KeyCode.PageDown))
                {
                    __instance.MenuList.SelectMenuItem(__instance.ConfirmKey);

                    SFXController.Instance.PlaySFX(SFXController.Instance.SFXMenuConfirm);

                    return false;
                }
                return true;
            }
        }
    }
}
