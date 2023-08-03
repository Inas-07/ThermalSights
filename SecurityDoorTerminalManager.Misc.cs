using ExtraObjectiveSetup.BaseClasses;
using LevelGeneration;
using SecDoorTerminalInterface;
using GameData;
using Localization;
using EOSExt.SecurityDoorTerminal.Definition;

namespace EOSExt.SecurityDoorTerminal
{
    public sealed partial class SecurityDoorTerminalManager : ZoneDefinitionManager<SecurityDoorTerminalDefinition>
    {
        public bool TryGetZoneEntranceSecDoor(LG_Zone zone, out LG_SecurityDoor door)
        {
            if (zone == null)
            {
                door = null;
                return false;
            }
            if (zone.m_sourceGate == null)
            {
                door = null;
                return false;
            }
            if (zone.m_sourceGate.SpawnedDoor == null)
            {
                door = null;
                return false;
            }
            door = zone.m_sourceGate.SpawnedDoor.TryCast<LG_SecurityDoor>();
            return door != null;
        }

        protected override void AddDefinitions(ZoneDefinitionsForLevel<SecurityDoorTerminalDefinition> definitions)
        {
            // instantiate sec door terminal instance in order
            Sort(definitions);
            base.AddDefinitions(definitions);
        }

        public static (eDimensionIndex dimensionIndex, LG_LayerType layerType, eLocalZoneIndex localIndex) GlobalZoneIndexOf(SecDoorTerminal sdt) => (sdt.SpawnNode.m_dimension.DimensionIndex, sdt.SpawnNode.LayerType, sdt.SpawnNode.m_zone.LocalIndex);

        private void AddOverrideCommandWithAlarmText(SecDoorTerminal sdt)
        {
            if (sdt.CmdProcessor.HasRegisteredCommand(SecDoorTerminal.COMMAND_OVERRIDE)) return;

            string command_desc = $"<color=orange>{Text.Get(841)}</color>";

            if (sdt.LinkedDoorLocks.ChainedPuzzleToSolve != null && sdt.LinkedDoorLocks.ChainedPuzzleToSolve.Data.TriggerAlarmOnActivate)
            {
                command_desc = $"<color=orange>{string.Format(Text.Get(840), sdt.LinkedDoorLocks.ChainedPuzzleToSolve?.Data.PublicAlarmName)}</color>";
            }

            sdt.AddOverrideCommand(OVERRIDE_COMMAND, command_desc);
        }
    }
}
