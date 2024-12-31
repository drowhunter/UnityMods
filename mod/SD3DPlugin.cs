using BepInEx;
using BepInEx.Logging;

using HarmonyLib;

namespace com.drowmods.depth3dunhinged
{
    [BepInPlugin(MyGuid, PluginName, VersionString)]
    public class SD3DPlugin : BaseUnityPlugin
    {
        private const string MyGuid = "com.drowmods.depth3dunhinged";
        private const string PluginName = "Depth3DUnhinged";
        private const string VersionString = "1.0.0";

        public static readonly Harmony harmony = new Harmony(MyGuid);
        public static ManualLogSource Log;

        public void Awake()
        {
           
            harmony.PatchAll();
            Logger.LogInfo(PluginName + " " + VersionString + " " + "loaded.");
            Log = Logger;
            
        }
    }
}
