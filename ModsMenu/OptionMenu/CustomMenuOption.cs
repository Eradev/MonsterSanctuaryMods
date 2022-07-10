using System;
using System.Collections.Generic;

namespace eradev.monstersanctuary.ModsMenuNS.OptionMenu
{
    internal class CustomMenuOption
    {
        public string Name { get; }

        public string ModName { get; }

        public bool DisabledInGameMenu { get; }

        public Func<string> DisplayValueFunc { get; }

        public Action<string> OnValueSelectFunc { get; }

        public Action<int> OnValueChangeFunc { get; }

        public Func<List<string>> PossibleValuesFunc { get; }

        public Func<bool> DetermineDisabledFunc { get; }

        public Action SetDefaultValueFunc { get; }

        public CustomMenuOption(
            string modName,
            string name,
            bool disabledInGameMenu,
            Func<string> displayValueFunc,
            Action<string> onValueSelectFunc,
            Action<int> onValueChangeFunc,
            Func<List<string>> possibleValuesFunc,
            Func<bool> determineDisabledFunc,
            Action setDefaultValueFunc)
        {
            ModName = modName;
            Name = name;
            DisabledInGameMenu = disabledInGameMenu;
            DisplayValueFunc = displayValueFunc;
            OnValueSelectFunc = onValueSelectFunc;
            OnValueChangeFunc = onValueChangeFunc;
            PossibleValuesFunc = possibleValuesFunc;
            DetermineDisabledFunc = determineDisabledFunc;
            SetDefaultValueFunc = setDefaultValueFunc;
        }

        public string Key => $"{ModName}.{Name}";
    }
}
