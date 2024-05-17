using EOSExt.SecDoor.Definition;
using ExtraObjectiveSetup.BaseClasses;

namespace EOSExt.SecDoor
{
    public class SecDoorIntTextOverrideManager : ZoneDefinitionManager<SecDoorIntTextOverride>
    {
        public static SecDoorIntTextOverrideManager Current { get; } = new();

        protected override string DEFINITION_NAME => "SecDoorIntText";

        private SecDoorIntTextOverrideManager() { }

        static SecDoorIntTextOverrideManager()
        {

        }
    }
}
