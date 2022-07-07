using System;
using System.Linq;
using BepInEx;
using HarmonyLib;

namespace eradev.monstersanctuary.OnlyShowValidMonstersEvolution
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class OnlyShowValidMonstersEvolutionPlugin : BaseUnityPlugin
    {
        private void Awake()
        {
            new Harmony(PluginInfo.PLUGIN_GUID).PatchAll();

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        private static Catalyst CurrentCatalyst()
        {
            return UIController.Instance.EvolveMenu.CurrentCatalyst;
        }

        [HarmonyPatch(typeof(MonsterSelector), "UpdatePages")]
        private class MonsterSelectorUpdatePagesPatch
        {
            // ReSharper disable once InconsistentNaming
            private static bool Prefix(ref MonsterSelector __instance)
            {
                if (__instance.CurrentSelectType != MonsterSelector.MonsterSelectType.SelectEvolveTarget)
                {
                    return true;
                }

                var numberEligibleMonsters =
                    PlayerController.Instance.Monsters.Active.Count(x => CurrentCatalyst().EvolvesFromMonster(x)) +
                    PlayerController.Instance.Monsters.Inactive.Count(x => CurrentCatalyst().EvolvesFromMonster(x));

                var totalPages = (int)Math.Ceiling((decimal)numberEligibleMonsters / __instance.MonstersPerRow * __instance.RowCount);

                AccessTools.FieldRefAccess<int>(typeof(MonsterSelector), "totalPages").Invoke(__instance) = totalPages;

                __instance.PageText.gameObject.SetActive(totalPages > 1);

                return false;
            }
        }

        [HarmonyPatch(typeof(MonsterSelector), "ShowMonsters")]
        private class MonsterSelectorShowMonstersPatch
        {
            // ReSharper disable once InconsistentNaming
            private static bool Prefix(ref MonsterSelector __instance)
            {
                if (__instance.CurrentSelectType != MonsterSelector.MonsterSelectType.SelectEvolveTarget)
                {
                    return true;
                }

                var currentPage =
                    AccessTools.FieldRefAccess<int>(typeof(MonsterSelector), "currentPage").Invoke(__instance);
                var totalPages =
                    AccessTools.FieldRefAccess<int>(typeof(MonsterSelector), "totalPages").Invoke(__instance);

                __instance.PageText.text = $"{Utils.LOCA("Page")}{GameDefines.GetSpaceChar()}{currentPage + 1}/{totalPages}";
                __instance.MenuList.Clear();

                var monstersPerPage = __instance.MonstersPerRow * __instance.RowCount;
                var num2 = monstersPerPage * currentPage;

                var eligibleActiveMonsters = PlayerController.Instance.Monsters.Active.Where(x => CurrentCatalyst().EvolvesFromMonster(x)).ToList();
                var eligibleInactiveMonsters =
                    PlayerController.Instance.Monsters.Inactive.Where(x => CurrentCatalyst().EvolvesFromMonster(x)).ToList();

                if (currentPage == 0)
                {
                    var index = 0;
                    foreach (var monster in eligibleActiveMonsters)
                    {
                        var menuListItem = __instance.MenuList.AddDisplayable(monster, index, 0);
                        menuListItem.GetComponent<MonsterSelectorView>().ShowMonster(monster);

                        AccessTools.Method(typeof(MonsterSelector), "UpdateDisabledStatus")
                            .Invoke(__instance, new object[]
                            {
                                menuListItem.GetComponent<MonsterSelectorView>()
                            });

                        index++;
                    }
                }
                else
                {
                    num2 -= eligibleActiveMonsters.Count;
                }

                for (var index = 0; index < eligibleInactiveMonsters.Count - num2 && __instance.MenuList.DisplayableItemCount < monstersPerPage; ++index)
                {
                    var monster = index < eligibleInactiveMonsters.Count - num2
                        ? eligibleInactiveMonsters[index + num2]
                        : null;

                    if (monster == null)
                    {
                        break;
                    }

                    var num4 = currentPage == 0
                        ? index + eligibleActiveMonsters.Count
                        : index;

                    var menuListItem = __instance.MenuList.AddDisplayable(monster, num4 % __instance.MonstersPerRow, num4 / 6);

                    menuListItem.GetComponent<MonsterSelectorView>().ShowMonster(monster);

                    AccessTools.Method(typeof(MonsterSelector), "UpdateDisabledStatus")
                        .Invoke(__instance, new object[]
                        {
                            menuListItem.GetComponent<MonsterSelectorView>()
                        });
                }

                return false;
            }
        }


    }
}
