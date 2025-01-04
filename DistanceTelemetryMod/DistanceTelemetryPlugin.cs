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
                WingsEnabled = car_logic.Wings_.enabled,
                WingsOpen = car_logic.Wings_.WingsOpen_,
                Mass = cRigidbody.mass,
                Finished = car.PlayerDataLocal_.Finished_,
                AllWheelsOnGround = car_logic.CarStats_.AllWheelsContacting_,
                Grip = car_logic.CarDirectives_.Grip_,
                isActiveAndEnabled = car.isActiveAndEnabled,
                TireFL = new Tire { IsInContact = car_logic.CarStats_.WheelFL_.IsInContactSmooth_, LocalPosition = car_logic.CarStats_.WheelFL_.hubTrans_.localPosition.y },
                TireFR = new Tire { IsInContact = car_logic.CarStats_.WheelFR_.IsInContactSmooth_, LocalPosition = car_logic.CarStats_.WheelFR_.hubTrans_.localPosition.y },
                TireBL = new Tire { IsInContact = car_logic.CarStats_.wheelBL_.IsInContactSmooth_, LocalPosition = car_logic.CarStats_.wheelBL_.hubTrans_.localPosition.y },
                TireBR = new Tire { IsInContact = car_logic.CarStats_.WheelBR_.IsInContactSmooth_, LocalPosition = car_logic.CarStats_.WheelBR_.hubTrans_.localPosition.y },
            };

            udp.Send(data);

            _packetId++;

            //log.LogInfo($"[{_packetId}] - VelocityX: {data.VelocityX:F4}, VelocityY: {data.VelocityY:F4}, VelocityZ: {data.VelocityZ:F4}, Yaw: {data.Yaw:F4}, Pitch: {data.Pitch:F4}, Roll: {data.Roll:F4}");
            //log.LogInfo($"[{_packetId}] - KPH: {data.KPH:F4}, Yaw: {data.Yaw:F4}, Pitch: {data.Pitch:F4}, Roll: {data.Roll:F4}");
            //log.LogInfo($"[{_packetId}] - KPH: {data.KPH:F4}, Yaw: {data.Yaw:F4}, Pitch: {data.Pitch:F4}, Roll: {data.Roll:F4}");
            //log.LogInfo($"[{_packetId}] - VX: {data.Velocity.x:F4}, VY: {data.Velocity.y:F4}, VZ: {data.Velocity.z:F4}");
            //log.LogInfo($"[{_packetId}] - Sway: {data.Sway:F4}, AccelX: {data.AccelX:F4}, AccelY: {data.AccelY:F4}, AccelZ: {data.AccelZ:F4}");
            

            //log.LogInfo($"[{_packetId}] - Boost: {data.Boost}, WingsEnabled: {data.WingsEnabled}, Mass: {data.Mass:F4}, Finished: {data.Finished}");
            //log.LogInfo($"[{_packetId}] - AllWheelsOnGround: {data.AllWheelsOnGround}, isActiveAndEnabled: {data.isActiveAndEnabled}, Grip: {data.Grip}");

            //log.LogInfo($"[{_packetId}] - TireFL: {data.TireFL.SuspensionDistance:F4}, TireFR: {data.TireFR.SuspensionDistance:F4}, TireBL: {data.TireBL.SuspensionDistance:F4}, TireBR: {data.TireBR.SuspensionDistance:F4}");

            //log.LogInfo($"[{_packetId}] - TireFL: {data.TireFL.IsInContact}, TireFR: {data.TireFR.IsInContact}, TireBL: {data.TireBL.IsInContact}, TireBR: {data.TireBR.IsInContact}");

            //log.LogInfo($"[{_packetId}] - TireFL: {data.TireFL.SpringForce:F4}, TireFR: {data.TireFR.SpringForce:F4}, TireBL: {data.TireBL.SpringForce:F4}, TireBR: {data.TireBR.SpringForce:F4}");

            //log.LogInfo($"[{_packetId}] - TireFL: {data.TireFL.LocalPosition}, TireFR: {data.TireFR.LocalPosition}, TireBL: {data.TireBL.LocalPosition}, TireBR: {data.TireBR.LocalPosition}");

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
        public bool WingsEnabled;
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
        public bool IsInContact;
        public float LocalPosition;
    }
}
