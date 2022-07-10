using System;
using System.Collections.Generic;
using System.Linq;

namespace eradev.monstersanctuary.ModsMenuNS.OptionMenu
{
    internal static class OptionsMenuHelper
    {
        public static readonly List<CustomMenuOption> CustomMenuOptions = new();

        public static void AddOptionToMenu(
            string modName,
            string optionName,
            Func<string> displayValueFunc,
            Action<string> onValueSelectFunc = null,
            Action<int> onValueChangeFunc = null,
            Func<List<string>> possibleValuesFunc = null,
            bool disabledInGameMenu = false,
            Func<bool> determineDisabledFunc = null,
            Action setDefaultValueFunc = null)
        {
            if (string.IsNullOrWhiteSpace(modName))
            {
                ModsMenu.LogError("You must pass a valid mod name.");

                return;
            }

            var optionKey = $"{modName}.{optionName}";

            if (CustomMenuOptions.Any(x => x.Key == optionKey))
            {
                ModsMenu.LogError($"Option \"{optionKey}\" already exist.");

                return;
            }

            CustomMenuOptions.Add(new CustomMenuOption(
                modName,
                optionName,
                disabledInGameMenu,
                displayValueFunc,
                onValueSelectFunc,
                onValueChangeFunc,
                possibleValuesFunc,
                determineDisabledFunc,
                setDefaultValueFunc));

            ModsMenu.LogDebug($"Added option \"{optionKey}\".");
        }
    }
}
