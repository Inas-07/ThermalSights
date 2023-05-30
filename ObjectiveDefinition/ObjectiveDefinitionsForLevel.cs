using GameData;
using LevelGeneration;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ExtraObjectiveSetup.ObjectiveDefinition
{
    public class BaseDefinition 
    {
        [JsonPropertyOrder(-2)] // prioritize base class property
        public eDimensionIndex DimensionIndex { get; set; }
        
        [JsonPropertyOrder(-2)]
        public LG_LayerType LayerType { get; set; }

        [JsonPropertyOrder(-2)]
        public eLocalZoneIndex LocalIndex { get; set; }

        [JsonPropertyOrder(-2)]
        public uint InstanceIndex { get; set; } = uint.MaxValue;
    }


    public class ObjectiveDefinitionsForLevel<T> where T : BaseDefinition, new()
    {
        public uint MainLevelLayout { set; get; } = 0;

        public List<T> Definitions { set; get; } = new() { new() };
    }
}
