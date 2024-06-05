using ChainedPuzzles;
using HarmonyLib;

namespace ThermalSights.Patches
{
    internal static class CP_Bioscan_Core_Setup
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CP_Bioscan_Core), nameof(CP_Bioscan_Core.Setup))]
        private static void Post_CaptureBioscanVisual(CP_Bioscan_Core __instance)
        {
            TSAManager.Current.RegisterPuzzleVisual(__instance);
        }
    }
}
