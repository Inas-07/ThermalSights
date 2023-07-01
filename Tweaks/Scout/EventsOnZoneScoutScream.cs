using GameData;
using System.Collections.Generic;

namespace ExtraObjectiveSetup.Tweaks.Scout
{
    public class EventsOnZoneScoutScream: BaseClasses.GlobalZoneIndex
    {

        public bool SuppressVanillaScoutWave { get; set; } = false;

        public List<WardenObjectiveEventData> EventsOnScoutScream { get; set; } = new();
    }
}
