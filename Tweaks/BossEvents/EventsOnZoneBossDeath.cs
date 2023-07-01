using GameData;
using System.Collections.Generic;

namespace ExtraObjectiveSetup.Tweaks.BossEvents
{
    public class EventsOnZoneBossDeath: BaseClasses.GlobalZoneIndex
    {
        public List<uint> BossIDs { set; get; } = new() { 29, 36, 37 };

        public List<WardenObjectiveEventData> EventsOnBossDeath { set; get; } = new(); 
    }
}
