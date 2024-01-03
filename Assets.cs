using GTFO.API;
using UnityEngine;

namespace EOSExt.SecuritySensor
{
    internal static class Assets
    {
        public static GameObject CircleSensor { get; private set; }

        public static GameObject MovableSensor { get; private set; }

        public static void Init()
        {
            CircleSensor = AssetAPI.GetLoadedAsset<GameObject>("Assets/SecuritySensor/CircleSensor.prefab");
            MovableSensor = AssetAPI.GetLoadedAsset<GameObject>("Assets/SecuritySensor/MovableSensor.prefab");
        }
    }
}
