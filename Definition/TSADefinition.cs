namespace ThermalSights.Definition
{
    public class TSADefinition
    {
        public float OffAimPixelZoom { get; set; } = 1.0f;

        public TSShader Shader { get; set; } = new();
    }
}
