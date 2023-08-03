using System.Collections.Generic;
using GameData;

namespace EOSExt.SecurityDoorTerminal.Definition
{
    public class SecurityDoorTerminalDefinition : ExtraObjectiveSetup.BaseClasses.GlobalZoneIndex
    {
        public SDTStateSetting StateSettings { get; set; } = new();

        public List<TerminalLogFileData> LocalLogFiles { set; get; } = new();

        public List<SDTCustomCommand> UniqueCommands { set; get; } = new() { new() };

        public TerminalPasswordData PasswordData { set; get; } = new();
    }
}
