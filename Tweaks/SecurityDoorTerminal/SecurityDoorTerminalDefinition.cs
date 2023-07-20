using System.Collections.Generic;
using GameData;
using SecDoorTerminalInterface;
namespace ExtraObjectiveSetup.Tweaks.SecurityDoorTerminal
{
    public class SecurityDoorTerminalDefinition : BaseClasses.GlobalZoneIndex
    {
        public CPSolvedBehaviour CPSolvedBehaviour { get; set; } = CPSolvedBehaviour.AddOpenCommand;

        // TODO: can i new `TerminalLogFileData`, `CustomTerminalCommand` here?
        public List<TerminalLogFileData> LocalLogFiles { set; get; } = new();

        public List<SDTCustomCommand> UniqueCommands { set; get; } = new() { new() }; 

        public SDTStartStateData StartingStateData { set; get; } = new();
    }
}
