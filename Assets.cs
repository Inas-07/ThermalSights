using GTFO.API;
using UnityEngine;

namespace EOSExt.SecuritySensor
{
    internal static class Assets
    {
        public static GameObject SecuritySensor { get; private set; }

        public static void Init()
        {
            SecuritySensor = AssetAPI.GetLoadedAsset<GameObject>("Assets/SecuritySensor/CircleSensor.prefab");
        }
    }
}
