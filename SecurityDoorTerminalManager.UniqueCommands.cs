using ExtraObjectiveSetup.BaseClasses;
using ExtraObjectiveSetup.BaseClasses.CustomTerminalDefinition;
using SecDoorTerminalInterface;
using GameData;
using ChainedPuzzles;
using GTFO.API.Extensions;
using EOSExt.SecurityDoorTerminal.Definition;
using ExtraObjectiveSetup.Utils;

namespace EOSExt.SecurityDoorTerminal
{
    public sealed partial class SecurityDoorTerminalManager : ZoneDefinitionManager<SecurityDoorTerminalDefinition>
    {
        private void BuildSDT_UniqueCommands(SecDoorTerminal sdt, SecurityDoorTerminalDefinition def)
        {
            def.TerminalSettings.UniqueCommands.ForEach(cmd => EOSTerminalUtils.AddUniqueCommand(sdt.ComputerTerminal, cmd));
        }

        private void BuildLevelSDTs_UniqueCommands()
        {
            levelSDTs.ForEach((tp) => BuildSDT_UniqueCommands(tp.sdt, tp.def));
        }
    }
}
