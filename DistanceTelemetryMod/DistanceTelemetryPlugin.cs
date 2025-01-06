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
            
            Vector3 localAngularVelocity = cRigidbody.transform.InverseTransformDirection(angularVelocity);            
            Vector3 localVelocity = cRigidbody.transform.InverseTransformDirection(cRigidbody.velocity);
            
            Vector3 lgforce = (localVelocity - previousLocalVelocity) / Time.fixedDeltaTime / 9.81f;
            previousLocalVelocity = localVelocity;

            var centripetalForce = localVelocity.magnitude * localAngularVelocity.magnitude * Math.Sign(localAngularVelocity.y);
            
            // equivalent to above

            //var radius = cRigidbody.velocity.magnitude / cRigidbody.angularVelocity.magnitude;
            //var centripetalForce3 = cRigidbody.mass * cRigidbody.velocity.sqrMagnitude / radius;


            //Vector3 gforce = (cRigidbody.velocity - previousVelocity) / Time.fixedDeltaTime / 9.81f;
            //previousVelocity = cRigidbody.velocity;

            //var euler = rotation.ToEuler(true);

        


            var data = new DistanceTelemetryData
            {
                PacketId = _packetId,
                KPH = car_logic.CarStats_.GetKilometersPerHour(),
                Mass = cRigidbody.mass,
                Yaw = Maths.HemiCircle(rotation.eulerAngles.y),
                Pitch = Maths.HemiCircle(rotation.eulerAngles.x),
                Roll = -Maths.HemiCircle(rotation.eulerAngles.z),
                //Yaw = euler.y,
                //Pitch = euler.x,
                //Roll = euler.z,
                Sway = centripetalForce,
                Velocity = localVelocity,
                AngularDrag = cRigidbody.angularDrag,
                Accel = lgforce,
                Inputs = new Inputs
                {
                    Gas = car_logic.CarDirectives_.Gas_,
                    Brake = car_logic.CarDirectives_.Brake_,
                    Steer = car_logic.CarDirectives_.Steer_,
                    Boost = car_logic.CarDirectives_.Boost_,
                    Grip = car_logic.CarDirectives_.Grip_,
                    Wings = car_logic.Wings_.WingsOpen_
                },         
                
                Finished = car.PlayerDataLocal_.Finished_,
                AllWheelsOnGround = car_logic.CarStats_.AllWheelsContacting_,                
                isActiveAndEnabled = car.isActiveAndEnabled,
                Grav = cRigidbody.useGravity,
                
                TireFL = new Tire { Contact = car_logic.CarStats_.WheelFL_.IsInContactSmooth_, Position = car_logic.CarStats_.WheelFL_.hubTrans_.localPosition.y, Suspension = CalcSuspension(car_logic.CarStats_.WheelFL_) },
                TireFR = new Tire { Contact = car_logic.CarStats_.WheelFR_.IsInContactSmooth_, Position = car_logic.CarStats_.WheelFR_.hubTrans_.localPosition.y, Suspension = CalcSuspension(car_logic.CarStats_.WheelFR_) },
                TireBL = new Tire { Contact = car_logic.CarStats_.wheelBL_.IsInContactSmooth_, Position = car_logic.CarStats_.wheelBL_.hubTrans_.localPosition.y, Suspension = CalcSuspension(car_logic.CarStats_.wheelBL_) },
                TireBR = new Tire { Contact = car_logic.CarStats_.WheelBR_.IsInContactSmooth_, Position = car_logic.CarStats_.WheelBR_.hubTrans_.localPosition.y, Suspension = CalcSuspension(car_logic.CarStats_.WheelBR_) },
            };

            udp.Send(data);

            _packetId++;

            float CalcSuspension(NitronicCarWheel wheel)
            {
                var pos = Math.Abs(wheel.hubTrans_.localPosition.y);
                var suspension = wheel.SuspensionDistance_;

                
                var frac =  pos / suspension;

                var s = Maths.EnsureMapRange(pos, 0, suspension, 1 , -1);

                return (float) s;

            }

        }


        [HarmonyCleanup]
        public static void CleanUp()
        {
            FileLog.Log("Cleaning Up");
            udp?.Dispose();
        }

        

    }

    public static class math
    {
        /// <summary>
        /// Convert quaternion to Euler angles
        /// </summary>
        /// <param name="q"></param>(float pitch, float yaw, float roll)
        /// <returns>pitch(x-rotation), yaw (y-rotation) , roll (z-rotation)</returns>
        public static Vector3 ToEuler(this Quaternion q, bool returnDegrees = true)
        {

            var p = (float)q.ToPitch();
            var y = (float)q.ToYaw();
            var r = (float)q.ToRoll();

            if (!returnDegrees)
                return new Vector3(p, y, r);

            // Convert the angles from radians to degrees
            return new Vector3(p * Mathf.Rad2Deg, y * Mathf.Rad2Deg, r * Mathf.Rad2Deg);

        }

        

        private static double ToPitch(this Quaternion q)
        {
            double num = 2.0 * (q.x * q.y + q.w * q.y);
            double num2 = 2.0 * (q.w * q.x - q.y * q.z);
            double num3 = 1.0 - 2.0 * (q.x * q.x + q.y * q.y);
            return Math.Atan2(num2, Math.Sqrt(num * num + num3 * num3));
        }

        private static double ToYaw(this Quaternion q) => Math.Atan2(2.0 * (q.x * q.y + q.w * q.y), 1.0 - 2.0 * (q.x * q.x + q.y * q.y));

        private static double ToRoll(this Quaternion q) => Math.Atan2(2.0 * (q.x * q.y + q.w * q.z), 1.0 - 2.0 * (q.x * q.x + q.z * q.z));
    }
}
