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

        private void AddOverrideCommandWithAlarmText(SecDoorTerminal sdt)
        {
            if (sdt.CmdProcessor.HasRegisteredCommand(SecDoorTerminal.COMMAND_OVERRIDE)) return;

            string command_desc = $"<color=orange>{Text.Get(841)}</color>";

            if (sdt.LinkedDoorLocks.ChainedPuzzleToSolve != null && sdt.LinkedDoorLocks.ChainedPuzzleToSolve.Data.TriggerAlarmOnActivate)
            {
                command_desc = $"<color=orange>{string.Format(Text.Get(840), sdt.LinkedDoorLocks.ChainedPuzzleToSolve?.Data.PublicAlarmName)}</color>";

                int idx = command_desc.IndexOf('[');
                if(idx >= 0 && idx < command_desc.Length)
                {
                    command_desc = command_desc.Insert(idx, "\n");
                }
            }

            sdt.AddOverrideCommand(OVERRIDE_COMMAND, command_desc);
        }
    }
}
