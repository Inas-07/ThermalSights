using SecDoorTerminalInterface;

namespace EOSExt.SecurityDoorTerminal.Definition
{
    public class SDTSettings
    {
        public bool InteractableWhenDoorIsLocked { get; set; } = false;

        public bool AddOverrideCommandWhenDoorUnlocked { get; set; } = true;

        public CPSolvedBehaviour CPSolvedBehaviour { get; set; } = CPSolvedBehaviour.AddOpenCommand;

    }
}
