using LevelGeneration;
using GameData;

namespace ExtraObjectiveSetup.Utils
{
    public static class Helper
    {
        public static System.Collections.Generic.List<T> cast<T>(this Il2CppSystem.Collections.Generic.List<T> list)
        {
            System.Collections.Generic.List<T> res = new();

            foreach(T obj in list)
            {
                res.Add(obj);
            }

            return res;
        }
   
        public static LG_ComputerTerminal FindTerminal(eDimensionIndex dimensionIndex, LG_LayerType layerType, eLocalZoneIndex localIndex, int instanceIndex)
        {
            LG_Zone zone = null;
            if (!Builder.CurrentFloor.TryGetZoneByLocalIndex(dimensionIndex, layerType, localIndex, out zone) || zone == null)
            {
                EOSLogger.Error($"FindTerminal: Didn't find LG_Zone {dimensionIndex}, {layerType}, {localIndex}");
                return null;
            }

            if (zone.TerminalsSpawnedInZone == null || instanceIndex >= zone.TerminalsSpawnedInZone.Count)
            {
                EOSLogger.Error($"FindTerminal: Invalid terminal index {instanceIndex} - {(zone.TerminalsSpawnedInZone == null ? 0 : zone.TerminalsSpawnedInZone.Count)} terminals are spawned in {dimensionIndex}, {layerType}, {localIndex}");
                return null;
            }

            return instanceIndex < 0 ? null : zone.TerminalsSpawnedInZone[instanceIndex];
        }
    }
}

