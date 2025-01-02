using BepInEx;
using BepInEx.Logging;

using HarmonyLib;

using System.Net;

using TelemetryLibrary;

namespace com.drowmods.DistanceTelemetryMod
{
    [BepInPlugin(MyGuid, PluginName, VersionString)]
    public class DistanceTelemetryPlugin : BaseUnityPlugin
    {
        private const string MyGuid = "com.drowmods.DistanceTelemetryPlugin";
        internal const string PluginName = "DistanceTelemetryPlugin";
        private const string VersionString = "1.0.0";

        public static readonly Harmony harmony = new Harmony(MyGuid);
        public static ManualLogSource Log;

        public static void Echo(string caller, string message)
        {
            Log?.LogInfo(string.Format("[{0}] {1}", caller, message));
        }

        public void Awake()
        {

            harmony.PatchAll();

            Log = Logger;

            Log.LogInfo(string.Format("{0} {1} loaded.", PluginName, VersionString));
        }

        public void Update()
        {
            //Logger.LogInfo(string.Format("[{0}] Update", PluginName));
        }

    }


    [HarmonyPatch(typeof(LocalPlayerControlledCar))]
    internal class LocalPlayerControlledCarPatches
    {

        static int _packetId = 0;
        static ManualLogSource log;
        static UdpTelemetry<DistanceTelemetryData> udp;

        static LocalPlayerControlledCarPatches()
        {
            FileLog.Log(string.Format("[{0}] Constructor = {1}", nameof(LocalPlayerControlledCarPatches), DistanceTelemetryPlugin.Log == null));
            log = DistanceTelemetryPlugin.Log;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(LocalPlayerControlledCar.Start))]
        public static void Start_Postfix(LocalPlayerControlledCar __instance)
        {
            if (log == null)
            {
                log = Logger.CreateLogSource("LocalPlayerControlledCarPatches");
                log.LogDebug("Start_Postfix - log was null");
            }

            udp = new UdpTelemetry<DistanceTelemetryData>(new UdpTelemetryConfig
            {
                SendAddress = new IPEndPoint(IPAddress.Loopback, 12345)
            });

        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(LocalPlayerControlledCar.UpdateLocal))]
        public static void Update_Postfix(LocalPlayerControlledCar __instance)
        {
            var car = __instance;
            //var car_rb = car.GetComponent<Rigidbody>();
            //var car_logic = car.GetComponent<CarLogic>();
            //System.IO.File.WriteAllText("E:\\SteamLibrary\\SteamApps\\common\\Distance\\test.txt", "Update_Postfix");
            //if (log == null)
            //{
            //    FileLog.Log("Update_Postfix - log was null");
            //    //log = DistanceTelemetryPlugin.Log;
            //    //
            //}

            var data = new DistanceTelemetryData
            {
                PacketId = _packetId,
                X = car.transform.position.x,
                Y = car.transform.position.y,
                Z = car.transform.position.z,
                Yaw = hemiCircle(car.transform.rotation.eulerAngles.y),
                Pitch = hemiCircle(car.transform.rotation.eulerAngles.x),
                Roll = hemiCircle(-car.transform.rotation.eulerAngles.z)
            };

            udp.Send(data);

            _packetId++;

            log.LogInfo($"[{_packetId}] - X: {data.X:F4}, Y: {data.Y:F4}, Z: {data.Z:F4}, Yaw: {data.Yaw:F4}, Pitch: {data.Pitch:F4}, Roll: {data.Roll:F4}");
        }

        [HarmonyCleanup]
        public static void CleanUp()
        {
            FileLog.Log("Cleaning Up");
            udp?.Dispose();
        }

        private static float hemiCircle(float angle)
        {
            return angle >= 180 ? angle - 360 : angle;
        }

    }

    public struct DistanceTelemetryData
    {
        public int PacketId;
        public float X;
        public float Y;
        public float Z;
        public float Yaw;
        public float Pitch;
        public float Roll;
        
    }
}
