using System.Collections.Generic;
using GameData;

namespace EOSExt.SecuritySensor.Definition
{
    public class SensorGroupSettings
    {
        public SensorColor Color { get; set; } = new SensorColor();

        public List<SensorSettings> SensorGroup { set; get; } = new() { new() };

        public List<WardenObjectiveEventData> EventsOnTrigger { set; get; } = new() { };
    }
}
