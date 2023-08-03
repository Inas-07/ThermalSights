using SecDoorTerminalInterface;

namespace EOSExt.SecurityDoorTerminal.Definition
{
    public enum OverrideCmdAccess // Override command accessibility
    {
        ALWAYS,
        ON_UNLOCK,
    }

    public class SDTStateSetting_Locked
    {
        public bool AccessibleWhenLocked { get; set; } = false;

        public bool AccessibleWhenUnlocked { get; set; } = true;
    }

    public class SDTStateSetting
    {
        public SDTStateSetting_Locked LockedStateSetting { get; set; } = new();

        public OverrideCmdAccess OverrideCommandAccessibility { get; set; } = OverrideCmdAccess.ON_UNLOCK;
    }
}
