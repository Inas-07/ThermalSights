using ExtraObjectiveSetup.BaseClasses;

namespace ExtraObjectiveSetup.Tweaks.TerminalPosition
{
    internal class TerminalPositionOverrideManager: InstanceDefinitionManager<TerminalPosition>
    {
        public static TerminalPositionOverrideManager Current = new();

        protected override string DEFINITION_NAME => "TerminalPosition";

        private TerminalPositionOverrideManager() { }

        static TerminalPositionOverrideManager()
        {

        }
    }
}
