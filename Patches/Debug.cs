using ExtraObjectiveSetup.Utils;
using GTFO.API;
using HarmonyLib;
using LevelGeneration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EOSExt.SecuritySensor.Patches
{
    [HarmonyPatch]
    internal class Debug
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(LG_CollisionWorldEventTrigger), nameof(LG_CollisionWorldEventTrigger.Update))]
        private static void Post(LG_CollisionWorldEventTrigger __instance) // this is not invoked at all
        {
            
        }

        static Debug()
        {

        }

    }
}
