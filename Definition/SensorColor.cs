using UnityEngine;

namespace EOSExt.SecuritySensor.Definition
{
    public class SensorColor
    {
        public float r { set; get; }

        public float g { set; get; }

        public float b { set; get; }

        public float a { set; get; }

        public Color toColor() => new Color(r, g, b, a);

        public SensorColor() { }
    }
}
