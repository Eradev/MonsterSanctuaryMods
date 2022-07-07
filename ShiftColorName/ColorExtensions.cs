using UnityEngine;

namespace eradev.monstersanctuary.ShiftColorName
{
    public static class ColorExtensions
    {
        // ReSharper disable once InconsistentNaming
        public static string ToHtmlRGBA(this Color input)
        {
            return ColorUtility.ToHtmlStringRGBA(input);
        }
    }
}
