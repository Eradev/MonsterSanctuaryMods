using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace eradev.monstersanctuary.DisplayUnhatchedEggs
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class DisplayUnhatchedEggsPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        [HarmonyPatch(typeof(Egg), "GetName")]
        private class EggGetNamePatch
        {
            private static void Postfix(
                // ReSharper disable once InconsistentNaming
                ref Egg __instance,
                // ReSharper disable once InconsistentNaming
                ref string __result)
            {
                var monster = __instance.Monster.GetComponent<Monster>();

                if (ProgressManager.Instance.HasMonterEntry(monster.ID) &&
                    ProgressManager.Instance.GetMonsterData(monster.ID).Hatched)
                {
                    return;
                }

                __result = $"* {__result}";
            }
        }
    }
}
