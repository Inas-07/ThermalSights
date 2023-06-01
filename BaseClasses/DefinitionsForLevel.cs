using GameData;
using LevelGeneration;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ExtraObjectiveSetup.BaseClasses
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

        public (eDimensionIndex, LG_LayerType, eLocalZoneIndex) GlobalZoneIndex => (DimensionIndex, LayerType, LocalIndex);
    }


    public class DefinitionsForLevel<T> where T : BaseDefinition, new()
    {
        public uint MainLevelLayout { set; get; } = 0;

        public List<T> Definitions { set; get; } = new() { new() };
    }
}
