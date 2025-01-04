using BepInEx;
using BepInEx.Logging;

using Events.Player;

using HarmonyLib;

using System;
using System.Net;
using System.Runtime.InteropServices;

using TelemetryLibrary;

using UnityEngine;
using Logger = BepInEx.Logging.Logger;

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

    
    //internal class GameManagerPatches
    //{
    //    static ManualLogSource log;

    //    public GameManagerPatches()
    //    {
    //        log = Logger.CreateLogSource("GameManagerPatches");
    //    }

    //    [HarmonyPatch(typeof(GameManager), "FixedUpdate")]
    //    private class GameManagerPatches_GameManager_FixedUpdate
    //    {
    //        public static bool blah = false;



    //        private static void Postfix(GameManager __instance)
    //        {
    //            //DistanceTelemetryPlugin.Echo(nameof(GameManagerPatches_GameManager_FixedUpdate), "Postfix");
    //            blah = GameManager.IsInGameModeScene_;
    //            log.LogDebug("GameManagerPatches_GameManager_FixedUpdate IsInGameModeScene_" + blah);
                
    //        }
    //    }

    //}

    [HarmonyPatch(typeof(LocalPlayerControlledCar))]
    internal class LocalPlayerControlledCarPatches
    {

        static int _packetId = 0;
        static ManualLogSource log;
        static UdpTelemetry<DistanceTelemetryData> udp;

        static bool PlayerEvent_Finished = false;
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

            //var playerEvents = __instance.GetComponent<PlayerEvents>();
            //playerEvents.Subscribe<Finished.Data>(data =>
            //{
            //    log.LogInfo("Finished.Data");
            //}); 

        }

        private static Vector3 previousVelocity = Vector3.zero;
        private static Vector3 previousLocalVelocity = Vector3.zero;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(LocalPlayerControlledCar.UpdateLocal))]
        public static void Update_Postfix(LocalPlayerControlledCar __instance)
        {
            var car = __instance;
            var cRigidbody = car.GetComponent<Rigidbody>();
            var car_logic = car.GetComponent<CarLogic>();

            //var ontrack = GameManager.IsInGameModeScene_;


            Quaternion rotation = cRigidbody.rotation;
            Vector3 eulerAngles = rotation.eulerAngles;
            Vector3 angularVelocity = cRigidbody.angularVelocity;
            
            Vector3 localAngularVelocity = cRigidbody.transform.InverseTransformDirection(cRigidbody.angularVelocity);            
            Vector3 localVelocity = cRigidbody.transform.InverseTransformDirection(cRigidbody.velocity);
            
            Vector3 lgforce = (localVelocity - previousLocalVelocity) / Time.fixedDeltaTime / 9.81f;
            previousLocalVelocity = localVelocity;

            var centripetalForce = cRigidbody.velocity.magnitude * cRigidbody.angularVelocity.magnitude * Math.Sign(localAngularVelocity.y);
             // equivalent to above

            //var radius = cRigidbody.velocity.magnitude / cRigidbody.angularVelocity.magnitude;
            //var centripetalForce3 = cRigidbody.mass * cRigidbody.velocity.sqrMagnitude / radius;


            Vector3 gforce = (cRigidbody.velocity - previousVelocity) / Time.fixedDeltaTime / 9.81f;
            previousVelocity = cRigidbody.velocity;
            
            //cRigidbody.velocity.magnitude;
            var velocity = cRigidbody.velocity;

            var data = new DistanceTelemetryData
            {
                PacketId = _packetId,
                KPH = car_logic.CarStats_.GetKilometersPerHour(),
                Yaw = hemiCircle(car.transform.rotation.eulerAngles.y),
                Pitch = hemiCircle(car.transform.rotation.eulerAngles.x),
                Roll = -hemiCircle(car.transform.rotation.eulerAngles.z),
                Sway = centripetalForce,
                Velocity = localVelocity,                
                Accel = lgforce,
                Boost = car_logic.CarDirectives_.Boost_,
                //OnTrack = ontrack,
                WingsOpen = car_logic.Wings_.WingsOpen_,
                Mass = cRigidbody.mass,
                Finished = car.PlayerDataLocal_.Finished_,
                AllWheelsOnGround = car_logic.CarStats_.AllWheelsContacting_,
                Grip = car_logic.CarDirectives_.Grip_,
                isActiveAndEnabled = car.isActiveAndEnabled,
                TireFL = new Tire { Contact = car_logic.CarStats_.WheelFL_.IsInContactSmooth_, Position = car_logic.CarStats_.WheelFL_.hubTrans_.localPosition.y },
                TireFR = new Tire { Contact = car_logic.CarStats_.WheelFR_.IsInContactSmooth_, Position = car_logic.CarStats_.WheelFR_.hubTrans_.localPosition.y },
                TireBL = new Tire { Contact = car_logic.CarStats_.wheelBL_.IsInContactSmooth_, Position = car_logic.CarStats_.wheelBL_.hubTrans_.localPosition.y },
                TireBR = new Tire { Contact = car_logic.CarStats_.WheelBR_.IsInContactSmooth_, Position = car_logic.CarStats_.WheelBR_.hubTrans_.localPosition.y },
            };

            udp.Send(data);

            _packetId++;

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

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct DistanceTelemetryData
    {
        public int PacketId;
        public float KPH;        
        public float Yaw;
        public float Pitch;
        public float Roll;
        public float Sway;
        public Vector3 Velocity;        
        public Vector3 Accel;       
        public bool Boost;
        //public bool OnTrack;
        public bool WingsOpen;
        public float Mass;
        public bool Finished;
        public bool AllWheelsOnGround;
        public bool isActiveAndEnabled;
        public bool Grip;
        public Tire TireFL;
        public Tire TireFR;
        public Tire TireBL;
        public Tire TireBR;
    }

    internal struct Tire
    {
        public bool Contact;
        public float Position;
    }
}
