using System.Collections.Generic;
using GameData;

namespace EOSExt.SecuritySensor.Definition
{
    public class SensorGroupSettings
    {
        public List<SensorSettings> SensorGroup { set; get; } = new() { new() };

        public List<WardenObjectiveEventData> EventsOnTrigger { set; get; } = new() { };
    }
}
