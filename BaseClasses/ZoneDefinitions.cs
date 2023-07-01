using GameData;
using LevelGeneration;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ExtraObjectiveSetup.BaseClasses
{
    public class GlobalZoneIndex 
    {
        [JsonPropertyOrder(-10)] // prioritize base class property
        public eDimensionIndex DimensionIndex { get; set; }

        [JsonPropertyOrder(-10)]
        public LG_LayerType LayerType { get; set; }

        [JsonPropertyOrder(-10)]
        public eLocalZoneIndex LocalIndex { get; set; }

        public (eDimensionIndex, LG_LayerType, eLocalZoneIndex) GlobalZoneIndexTuple() => (DimensionIndex, LayerType, LocalIndex);
    
        public override string ToString() => $"{GlobalZoneIndexTuple}";
    }

    public class ZoneDefinitionsForLevel<T> where T : GlobalZoneIndex, new()
    {
        public uint MainLevelLayout { set; get; } = 0;

        public List<T> Definitions { set; get; } = new() { new() };
    }
}
