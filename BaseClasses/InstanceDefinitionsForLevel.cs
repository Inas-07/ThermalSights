using GameData;
using LevelGeneration;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ExtraObjectiveSetup.BaseClasses
{
    public class GlobalZoneIndex 
    {
        [JsonPropertyOrder(-2)] // prioritize base class property
        public eDimensionIndex DimensionIndex { get; set; }

        [JsonPropertyOrder(-2)]
        public LG_LayerType LayerType { get; set; }

        [JsonPropertyOrder(-2)]
        public eLocalZoneIndex LocalIndex { get; set; }

        public (eDimensionIndex, LG_LayerType, eLocalZoneIndex) GlobalZoneIndexTuple() => (DimensionIndex, LayerType, LocalIndex);
    
        public override string ToString() => $"{GlobalZoneIndexTuple}";
    }

    public class BaseInstanceDefinition: GlobalZoneIndex
    {
        [JsonPropertyOrder(-2)]
        public uint InstanceIndex { get; set; } = uint.MaxValue;

        public override string ToString() => $"{GlobalZoneIndexTuple()}, Instance_{InstanceIndex}";
    }

    public class InstanceDefinitionsForLevel<T> where T : BaseInstanceDefinition, new()
    {
        public uint MainLevelLayout { set; get; } = 0;

        public List<T> Definitions { set; get; } = new() { new() };
    }
}
