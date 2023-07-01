using BepInEx.Unity.IL2CPP;
using ExtraObjectiveSetup.Utils;
using GTFO.API.JSON.Converters;
using System;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;

namespace ExtraObjectiveSetup.JSON
{
    public static class AWOUtil
    {
        public const string PLUGIN_GUID = "GTFO.AWO";

        public static JsonConverter AWOEventDataConverter { get; private set; } = null;
        public static bool IsLoaded { get; private set; } = false;

        static AWOUtil()
        {
            if (IL2CPPChainloader.Instance.Plugins.TryGetValue(PLUGIN_GUID, out var info))
            {
                try
                {
                    var ddAsm = info?.Instance?.GetType()?.Assembly ?? null;

                    if (ddAsm is null)
                        throw new Exception("Assembly is Missing!");

                    var types = ddAsm.GetTypes();
                    var converterType = types.First(t => t.Name == "JsonAPI");
                    if (converterType is null)
                        throw new Exception("Unable to Find JsonAPI Class");

                    var converterProp = converterType.GetProperty("EventDataConverter", BindingFlags.Public | BindingFlags.Static);

                    if (converterProp is null)
                        throw new Exception("Unable to Find Property: EventDataConverter");

                    AWOEventDataConverter = (JsonConverter)converterProp.GetValue(info);
                    IsLoaded = true;
                }
                catch (Exception e)
                {
                    EOSLogger.Error($"Exception thrown while reading data from GTFO.AWO: {e}");
                }
            }
        }
    }
}
