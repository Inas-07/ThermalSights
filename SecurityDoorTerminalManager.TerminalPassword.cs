using ExtraObjectiveSetup.BaseClasses;
using ExtraObjectiveSetup.Utils;
using EOSExt.SecurityDoorTerminal.Definition;

namespace EOSExt.SecurityDoorTerminal
{
    public sealed partial class SecurityDoorTerminalManager : ZoneDefinitionManager<SecurityDoorTerminalDefinition>
    {
        private void BuildLevelSDTs_Passwords()
        {
            levelSDTs.ForEach((tp) => EOSTerminalUtils.BuildPassword(tp.sdt.ComputerTerminal, tp.def.TerminalSettings.PasswordData));
        }
    }
}
