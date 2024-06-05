using HarmonyLib;
using System;

namespace ThermalSights.Patches
{
    [HarmonyPatch]
    internal static class FirstPersonItemHolder_SetWieldedItem
    {
        public static event Action<FirstPersonItemHolder, ItemEquippable> OnItemWielded;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FirstPersonItemHolder), nameof(FirstPersonItemHolder.SetWieldedItem))]
        private static void Post_SetWieldedItem(FirstPersonItemHolder __instance, ItemEquippable item)
        {
            TSAManager.Current.OnPlayerItemWielded(__instance, item);
            TSAManager.Current.SetPuzzleVisualsIntensity(1f);
            TSAManager.Current.SetCurrentThermalSightSettings(1f);
            OnItemWielded?.Invoke(__instance, item);
        }
    }
}
