using System.Collections.Generic;
using ExtraObjectiveSetup.Utils;

namespace EOSExt.SecuritySensor.Definition
{
    public enum SensorType
    {
        BASIC,
        MOVABLE,
    }

    public class SensorSettings
    {
        public Vec3 Position { get; set; } = new Vec3();

        public float Radius { get; set; } = 2.3f;

        public SensorType SensorType { get; set; } = SensorType.BASIC;

        public float MovingSpeedMulti { get; set; } = 1.0f;

        public List<Vec3> MovingPosition { get; set; } = new() { new() }; 
    }
}
