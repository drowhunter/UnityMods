using BepInEx;
using BepInEx.Logging;

using Events;
using Events.Car;
using Events.Game;
using Events.GameMode;
using Events.Player;
using Events.RaceEnd;

using System;
using System.Net;

using TelemetryLibrary;

using UnityEngine;

namespace com.drowmods.DistanceTelemetryMod
{
    [BepInPlugin(MyGuid, PluginName, VersionString)]
    public class DistanceTelemetryPlugin : BaseUnityPlugin
    {
        const string MyGuid = "com.drowmods.DistanceTelemetryPlugin";
        const string PluginName = "DistanceTelemetryPlugin";
        const string VersionString = "1.0.0";

        static ManualLogSource Log;

        
        PlayerEvents playerEvents;
        DistanceTelemetryData data;
        
        bool carDestroyed = false;

        
        UdpTelemetry<DistanceTelemetryData> udp;
        
        private Vector3 previousLocalVelocity = Vector3.zero;

        LocalPlayerControlledCar _car;

        private LocalPlayerControlledCar car 
        {
            get
            {
                if(_car == null)
                {                    
                    _car = G.Sys?.PlayerManager_?.localPlayers_?[0]?.playerData_?.localCar_;  
                    if(_car != null)
                    {
                        SubscribeToEvents();
                    }
                }

                return _car;
            }
        }

        

        public static void Echo(string caller, string message)
        {
            Log?.LogInfo(string.Format("[{0}] {1}", caller, message));
        }

        private void Awake()
        {
            //harmony.PatchAll();

            Log = Logger;

            Echo(nameof(DistanceTelemetryPlugin.Awake), string.Format("{0} {1} loaded.", PluginName, VersionString));

            udp = new UdpTelemetry<DistanceTelemetryData>(new UdpTelemetryConfig
            {
                SendAddress = new IPEndPoint(IPAddress.Loopback, 12345)
            });


            
        }

        private void OnDestroy()
        {
            Echo(nameof(DistanceTelemetryPlugin.OnDestroy), "Disposing Udp");
            udp?.Dispose();
        }

        private void OnEnable()
        {
            Echo(nameof(DistanceTelemetryPlugin.OnEnable), "Subscribing...");
            StaticEvent<LocalCarHitFinish.Data>.Subscribe(new StaticEvent<LocalCarHitFinish.Data>.Delegate(RaceEnded));
            StaticEvent<Go.Data>.Subscribe(new StaticEvent<Go.Data>.Delegate(RaceStarted));
            StaticEvent<PauseToggled.Data>.Subscribe(new StaticEvent<PauseToggled.Data>.Delegate(Toggle_Paused));
        }

        private void RaceStarted(Go.Data e)
        {
            data.IsRacing = true;
        }

        private void RaceEnded(LocalCarHitFinish.Data e)
        {
            data.IsRacing = false;
        }

        private void OnDisable()
        {
            Echo(nameof(DistanceTelemetryPlugin.OnDisable), "UnSubscribing...");

            StaticEvent<LocalCarHitFinish.Data>.Unsubscribe(new StaticEvent<LocalCarHitFinish.Data>.Delegate(RaceEnded));
            StaticEvent<Go.Data>.Unsubscribe(new StaticEvent<Go.Data>.Delegate(RaceStarted));
            StaticEvent<PauseToggled.Data>.Unsubscribe(new StaticEvent<PauseToggled.Data>.Delegate(Toggle_Paused));
            UnSubscribeFromEvents();
        }

        private void FixedUpdate()
        {
            if(car == null)
            {
                return;
            }

            data.IsCarEnabled = car.ExistsAndIsEnabled();

            var cRigidbody = car.GetComponent<Rigidbody>();
            var car_logic = car.carLogic_;
            
            
            Quaternion rotation = cRigidbody.transform.rotation;            

            //var localAngularVelocity = Quaternion.Inverse(rotation) * cRigidbody.angularVelocity;
            //var localVelocity = Quaternion.Inverse(rotation) * cRigidbody.velocity;
            

            var localAngularVelocity = cRigidbody.transform.InverseTransformDirection(cRigidbody.angularVelocity);
            var localVelocity = cRigidbody.transform.InverseTransformDirection(cRigidbody.velocity);

            
            Vector3 accel = (localVelocity - previousLocalVelocity) / Time.fixedDeltaTime / 9.81f;
            previousLocalVelocity = localVelocity;

            var cForce = localVelocity.magnitude * localAngularVelocity.magnitude * Math.Sign(localAngularVelocity.y);

            var pyr = rotation.ToPitchYawRoll();

            data.Pitch = pyr.pitch;
            data.Yaw = pyr.yaw;            
            data.Roll = pyr.roll;

            data.AngularVelocity = localAngularVelocity;


            data.KPH = car_logic.CarStats_.GetKilometersPerHour();
            
            data.cForce = cForce;
            
            data.Velocity = localVelocity;            
            data.Accel = accel;

            data.Boost = car_logic.CarDirectives_.Boost_;
            data.Grip = car_logic.CarDirectives_.Grip_;
            data.WingsOpen = car_logic.Wings_.WingsOpen_;
            

            //data.Finished = car.PlayerDataLocal_.Finished_;
            data.AllWheelsOnGround = car_logic.CarStats_.AllWheelsContacting_;
            data.IsCarIsActive = car.isActiveAndEnabled;
            data.IsGrav = cRigidbody.useGravity;
            data.TireFL = CalcSuspension(car_logic.CarStats_.WheelFL_);
            data.TireFR = CalcSuspension(car_logic.CarStats_.WheelFR_);
            data.TireBL = CalcSuspension(car_logic.CarStats_.wheelBL_);
            data.TireBR = CalcSuspension(car_logic.CarStats_.WheelBR_);

            data.IsCarDestroyed = carDestroyed;

            udp.Send(data);
            

            float CalcSuspension(NitronicCarWheel wheel)
            {
                var pos = Math.Abs(wheel.hubTrans_.localPosition.y);
                var suspension = wheel.SuspensionDistance_;


                var frac = pos / suspension;

                var s = Maths.EnsureMapRange(pos, 0, suspension, 1, -1);

                return (float)s;

            }

        }
        
        
        private void SubscribeToEvents()
        {
            Echo("SubscribeToEvents", "Subscribing to player events");
            playerEvents = car.playerDataLocal_.Events_;
            
            playerEvents.Subscribe(new InstancedEvent<Impact.Data>.Delegate(LocalVehicle_Collided));
            playerEvents.Subscribe(new InstancedEvent<Death.Data>.Delegate(LocalVehicle_Destroyed));
            playerEvents.Subscribe(new InstancedEvent<CarRespawn.Data>.Delegate(LocalVehicle_Respawn));
            playerEvents.Subscribe(new InstancedEvent<Explode.Data>.Delegate(LocalVehicle_Exploded));
        }

        private void Toggle_Paused(PauseToggled.Data e)
        {
            Log.LogDebug("Paused " + e.paused_);
            data.GamePaused = e.paused_;
        }

        private void UnSubscribeFromEvents()
        {
            Log.LogInfo("Unsubscribing from player events");
            playerEvents.Unsubscribe(new InstancedEvent<Impact.Data>.Delegate(LocalVehicle_Collided));
            playerEvents.Unsubscribe(new InstancedEvent<Death.Data>.Delegate(LocalVehicle_Destroyed));
            playerEvents.Unsubscribe(new InstancedEvent<CarRespawn.Data>.Delegate(LocalVehicle_Respawn));
            playerEvents.Unsubscribe(new InstancedEvent<Explode.Data>.Delegate(LocalVehicle_Exploded));
        }
        
        private void LocalVehicle_Collided(Impact.Data data)
        {
            Echo("LocalVehicle_Collided", "Collided");
            carDestroyed = true;
        }

        private void LocalVehicle_Destroyed(Death.Data data)
        {
            Echo("LocalVehicle_Destroyed", "Destroyed");
            carDestroyed = true;
        }

        private void LocalVehicle_Respawn(CarRespawn.Data data)
        {
            Echo("LocalVehicle_Respawn", "Respawned");
            carDestroyed = false;
        }

        private void LocalVehicle_Exploded(Explode.Data data)
        {
            Echo("LocalVehicle_Exploded", "Exploded");
            carDestroyed = true;
        }
    }


    internal static class QuatMath
    {
        public static PitchYawRoll ToPitchYawRoll(this Quaternion q)
        {
            var yaw = (float)  Math.Atan2(2 * q.y * q.w - 2 * q.x * q.z, 1 - 2 * q.y * q.y - 2 * q.z * q.z) * Mathf.Rad2Deg;
            var pitch = (float)Math.Atan2(2 * q.x * q.w - 2 * q.y * q.z, 1 - 2 * q.x * q.x - 2 * q.z * q.z) * Mathf.Rad2Deg;
            var roll = (float) Math.Asin (2 * q.x * q.y + 2 * q.z * q.w) * Mathf.Rad2Deg;

            return new PitchYawRoll(pitch, yaw, -roll);
        }

    }
}