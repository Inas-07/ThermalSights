using FirstPersonItem;
using HarmonyLib;
using System;

namespace ThermalSights.Patches
{
    [HarmonyPatch]
    internal static class FPIS_Aim_Update
    {
        public static event Action<FPIS_Aim, float> OnAimUpdate;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FPIS_Aim), nameof(FPIS_Aim.Update))]
        private static void Post_Aim_Update(FPIS_Aim __instance)
        {
            if (__instance.Holder.WieldedItem == null) return;

            float t = 1.0f - FirstPersonItemHolder.m_transitionDelta;
            if (!TSAManager.Current.IsGearWithThermal(TSAManager.Current.CurrentGearPID))
            {
                t = Math.Max(0.05f, t);
            }
            else
            {
                TSAManager.Current.SetCurrentThermalSightSettings(t);
            }

            TSAManager.Current.SetPuzzleVisualsIntensity(t);

            OnAimUpdate?.Invoke(__instance, t);
        }
    }
}
