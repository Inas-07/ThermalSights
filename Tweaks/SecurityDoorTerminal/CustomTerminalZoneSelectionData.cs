using GameData;
using System;
namespace ExtraObjectiveSetup.Tweaks.SecurityDoorTerminal
{
    public class CustomTerminalZoneSelectionData: BaseClasses.GlobalZoneIndex
    {
        public eSeedType SeedType { set; get; } = eSeedType.SessionSeed;

        public int TerminalIndex { set; get; } = 0;

        public int StaticSeed { set; get; } = 0;
    }
}
