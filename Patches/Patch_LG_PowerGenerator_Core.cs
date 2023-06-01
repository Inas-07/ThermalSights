using HarmonyLib;
using LevelGeneration;
using ExtraObjectiveSetup.Utils;
using GameData;
using ExtraObjectiveSetup.Instances;
using ExtraObjectiveSetup.Objectives.IndividualGenerator;

namespace ExtraObjectiveSetup.Patches
{
    [HarmonyPatch]
    class Patch_LG_PowerGenerator_Core
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(LG_PowerGenerator_Core), nameof(LG_PowerGenerator_Core.Setup))]
        private static void Post_PowerGenerator_Setup(LG_PowerGenerator_Core __instance)
        {
            if (PowerGeneratorInstanceManager.Current.IsGCGenerator(__instance)) return;

            uint zoneInstanceIndex = PowerGeneratorInstanceManager.Current.Register(__instance);
            var globalZoneIndex = PowerGeneratorInstanceManager.Current.GetGlobalZoneIndex(__instance);
            var def = IndividualGeneratorObjectiveManager.Current.GetDefinition(globalZoneIndex, zoneInstanceIndex);
            
            if (def == null) return;

            var position = def.Position.ToVector3();
            var rotation = def.Rotation.ToQuaternion();

            if (position != UnityEngine.Vector3.zero)
            {
                __instance.transform.position = position;
                __instance.transform.rotation = rotation;

                __instance.m_sound.UpdatePosition(position);

                EOSLogger.Debug($"LG_PowerGenerator_Core: modified position / rotation");
            }

            if (def.ForceAllowPowerCellInsertion)
            {
                __instance.SetCanTakePowerCell(true);
            }

            if (def.EventsOnInsertCell?.Count > 0)
            {
                __instance.OnSyncStatusChanged += new System.Action<ePowerGeneratorStatus>((status) => {
                    if (status == ePowerGeneratorStatus.Powered)
                    {
                        def.EventsOnInsertCell.ForEach(e => WardenObjectiveManager.CheckAndExecuteEventsOnTrigger(e, eWardenObjectiveEventTrigger.None, true));
                    }
                }); 
            }

            EOSLogger.Debug($"LG_PowerGenerator_Core: overriden, instance {zoneInstanceIndex} in {globalZoneIndex}");
        }
    }
}
