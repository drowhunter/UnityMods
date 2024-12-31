using BepInEx;
using BepInEx.Logging;

using HarmonyLib;

using System.IO.MemoryMappedFiles;

using UnityEngine;

namespace com.drowmods.DistanceTelemetryMod
{
    [BepInPlugin(MyGuid, PluginName, VersionString)]
    public class DistanceTelemetryPlugin : BaseUnityPlugin
    {
        private const string MyGuid = "com.drowmods.DistanceTelemetryPlugin";
        private const string PluginName = "DistanceTelemetryPlugin";
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


    [HarmonyPatch(typeof(LocalPlayerControlledCar))]
    internal class LocalPlayerControlledCarPatches
    {
        //static MemoryMappedFile mmf;
        //static MemoryMappedViewAccessor accessor;
        static int _packetId = 0;
        static ManualLogSource log;

        //public LocalPlayerControlledCarPatches()
        //{
        //    log = DistanceTelemetryPlugin.Log;
        //}

        [HarmonyPostfix]
        [HarmonyPatch(nameof(LocalPlayerControlledCar.Start))]
        public static void Start_Postfix(LocalPlayerControlledCar __instance)
        {
            DistanceTelemetryPlugin.Log.LogInfo("LocalPlayerControlledCar Start");
            //mmf = MemoryMappedFile.CreateOrOpen("FPGeneric", Marshal.SizeOf(typeof(DistanceTelemetryData)));

            //accessor = mmf.CreateViewAccessor();
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(LocalPlayerControlledCar.UpdateLocal))]
        public static void Update_Postfix(LocalPlayerControlledCar __instance)
        {
            var car = __instance;
            //var car_rb = car.GetComponent<Rigidbody>();
            //var car_logic = car.GetComponent<CarLogic>();
            //System.IO.File.WriteAllText("E:\\SteamLibrary\\SteamApps\\common\\Distance\\test.txt", "Update_Postfix");


            var data = new DistanceTelemetryData
            {
                X = car.transform.position.x,
                Y = car.transform.position.y,
                Z = car.transform.position.z,
                Yaw = car.transform.rotation.eulerAngles.y,
                Pitch = car.transform.rotation.eulerAngles.x,
                Roll = car.transform.rotation.eulerAngles.z
            };

            //accessor?.Write(0, ref data);
            _packetId++;
            DistanceTelemetryPlugin.Log.LogInfo($"{_packetId} - X: {data.X:F4}, Y: {data.Y:F4}, Z: {data.Z:F4}, Yaw: {data.Yaw:F4}, Pitch: {data.Pitch:F4}, Roll: {data.Roll:F4}");
        }

        [HarmonyCleanup]
        public static void CleanUp()
        {
            FileLog.Log("Cleaning Up");

            //mmf?.Dispose();

            //accessor?.Dispose();
        }
    }

    internal struct DistanceTelemetryData
    {
        public int DataId;
        public float X;
        public float Y;
        public float Z;
        public float Yaw;
        public float Pitch;
        public float Roll;
        
    }
}
