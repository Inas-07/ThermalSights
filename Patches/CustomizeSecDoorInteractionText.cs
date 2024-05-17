using EOSExt.SecDoor.Definition;
using EOSExt.SecDoor;
using ExtraObjectiveSetup.Utils;
using HarmonyLib;
using LevelGeneration;
using Il2CppSystem.Text;

namespace EOSExt.SecDoor.Patches
{
    [HarmonyPatch]
    internal class CustomizeSecDoorInteractionText
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(LG_SecurityDoor), nameof(LG_SecurityDoor.Setup))]
        private static void Post_Customize_SecDoor_Interaction_Text(LG_SecurityDoor __instance)
        {
            LG_SecurityDoor door = __instance;
            var dim = door.Gate.DimensionIndex;
            var layer = door.LinksToLayerType;
            var localIndex = door.LinkedToZoneData.LocalIndex;
            var def = SecDoorIntTextOverrideManager.Current.GetDefinition(dim, layer, localIndex);

            Interact_Timed intOpenDoor = __instance.m_locks.Cast<LG_SecurityDoor_Locks>()?.m_intOpenDoor;

            if (intOpenDoor == null || def == null || def.GlitchMode != GlitchMode.None) return;

            //if (state.status != eDoorStatus.Unlocked && state.status != eDoorStatus.Closed_LockedWithChainedPuzzle) return;
            door.m_sync.add_OnDoorStateChange(new System.Action<pDoorState, bool>((state, isRecall) =>
            {
                //EOSLogger.Warning($"OnSyncDoorStatusChange: {state.status}");

                string textToReplace = string.IsNullOrEmpty(def.TextToReplace) ? intOpenDoor.InteractionMessage : def.TextToReplace; ;

                StringBuilder sb = new();
                if (!string.IsNullOrEmpty(def.Prefix))
                {
                    sb.Append(def.Prefix).AppendLine();
                }

                sb.Append(textToReplace);

                if (!string.IsNullOrEmpty(def.Postfix))
                {
                    sb.AppendLine().Append(def.Postfix);
                }

                intOpenDoor.InteractionMessage = sb.ToString();

                EOSLogger.Debug($"SecDoorIntTextOverride: Override IntText. {def.LocalIndex}, {def.LayerType}, {def.DimensionIndex}");
            }));

            //EOSLogger.Warning($"SecDoorIntTextOverride: Override IntText. {def.LocalIndex}, {def.LayerType}, {def.DimensionIndex}");
        }
    }
}
