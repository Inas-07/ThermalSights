using ExtraObjectiveSetup.Utils;
using ExtraObjectiveSetup.BaseClasses;

namespace ExtraObjectiveSetup.Tweaks.TerminalPosition
{
    public class TerminalPosition: BaseDefinition
    {
        public Vec3 Position { get; set; } = new();

        public Vec3 Rotation { get; set; } = new();
    }
}
