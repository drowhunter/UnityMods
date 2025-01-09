using BepInEx;
using BepInEx.Logging;

using Events;
using Events.Game;
using Events.GameMode;
using Events.Player;
using Events.RaceEnd;

using System;
using System.Net;

using TelemetryLibrary;

using UnityEngine;

using static UIKeyBinding;

using Logger = BepInEx.Logging.Logger;

namespace com.drowmods.DistanceTelemetryMod
{
    [BepInPlugin(MyGuid, PluginName, VersionString)]
    public class DistanceTelemetryPlugin : BaseUnityPlugin
    {
        private const string MyGuid = "com.drowmods.DistanceTelemetryPlugin";
        internal const string PluginName = "DistanceTelemetryPlugin";
        private const string VersionString = "1.0.0";

        private bool paused;

        //public readonly Harmony harmony = new Harmony(MyGuid);
        public static ManualLogSource Log;

        private PlayerEvents playerEvents;

        private LocalPlayerControlledCar _car;
        private LocalPlayerControlledCar car 
        {
            get
            {
                if(_car == null)
                {
                    _car = G.Sys?.PlayerManager_?.localPlayers_?[0]?.playerData_?.localCar_;
                    if(_car != null)
                        SubscribeToEvents();
                }

                return _car;
            }
        }

        DistanceTelemetryData data;

        int _packetId = 0;
        ManualLogSource log;
        UdpTelemetry<DistanceTelemetryData> udp;
        private Vector3 previousVelocity = Vector3.zero;
        private Vector3 previousLocalVelocity = Vector3.zero;

        public static void Echo(string caller, string message)
        {
            Log?.LogInfo(string.Format("[{0}] {1}", caller, message));
        }

        private void Awake()
        {
            //harmony.PatchAll();

            Log = Logger;

            Log.LogInfo(string.Format("{0} {1} loaded.", PluginName, VersionString));

            udp = new UdpTelemetry<DistanceTelemetryData>(new UdpTelemetryConfig
            {
                SendAddress = new IPEndPoint(IPAddress.Loopback, 12345)
            });
        }

        

        private void FixedUpdate()
        {
            if(car == null)
            {
                return;
            }

            data.CarEnabled = car.ExistsAndIsEnabled();

            var cRigidbody = car.GetComponent<Rigidbody>();
            var car_logic = car.carLogic_;
            

            var gravup = car_logic.CarStats_.gravityUp_;
            //var car_logicL = car.carLogic_.CarLogicLocal_;
            //var ontrack = GameManager.IsInGameModeScene_;
            Quaternion rotation = car.transform.rotation;
            Vector3 eulerAngles = rotation.eulerAngles;
            Vector3 angularVelocity = cRigidbody.angularVelocity;


            //Quaternion.Inverse(rotation)


            Vector3 localAngularVelocity = cRigidbody.transform.InverseTransformDirection(angularVelocity);
            Vector3 localVelocity = cRigidbody.transform.InverseTransformDirection(cRigidbody.velocity);

            Vector3 lgforce = (localVelocity - previousLocalVelocity) / Time.fixedDeltaTime / 9.81f;
            previousLocalVelocity = localVelocity;

            var centripetalForce = localVelocity.magnitude * localAngularVelocity.magnitude * Math.Sign(localAngularVelocity.y);

            Quaternion localRotation = cRigidbody.transform.localRotation;

            //var euler = rotation.ToEuler(true);
            data.GravityUp = gravup;
            data.Yaw = TelemetryLibrary.Maths.HemiCircle(rotation.eulerAngles.y);
            data.Pitch = TelemetryLibrary.Maths.HemiCircle(rotation.eulerAngles.x);
            data.Roll = -TelemetryLibrary.Maths.HemiCircle(rotation.eulerAngles.z);
            data.LocalRot = new Vector3(
                TelemetryLibrary.Maths.HemiCircle(localRotation.eulerAngles.x),
                TelemetryLibrary.Maths.HemiCircle(localRotation.eulerAngles.y), 
                TelemetryLibrary.Maths.HemiCircle(localRotation.eulerAngles.z));


            //Yaw = euler.y,
            //Pitch = euler.x,
            //Roll = euler.z,

            data.PacketId = _packetId;
            data.GamePaused = !paused;
            data.RaceStarted = raceStarted;

            data.KPH = car_logic.CarStats_.GetKilometersPerHour();
            data.Mass = cRigidbody.mass;
            data.Sway = centripetalForce;
            data.Velocity = localVelocity;
            data.AngularDrag = cRigidbody.angularDrag;
            data.Accel = lgforce;
            data.Inputs = new Inputs
            {
                Gas = car_logic.CarDirectives_.Gas_,
                Brake = car_logic.CarDirectives_.Brake_,
                Steer = car_logic.CarDirectives_.Steer_,
                Boost = car_logic.CarDirectives_.Boost_,
                Grip = car_logic.CarDirectives_.Grip_,
                Wings = car_logic.Wings_.WingsOpen_
            };

            data.Finished = car.PlayerDataLocal_.Finished_;
            data.AllWheelsOnGround = car_logic.CarStats_.AllWheelsContacting_;
            data.isActiveAndEnabled = car.isActiveAndEnabled;
            data.Grav = cRigidbody.useGravity;
            data.TireFL = new Tire { Contact = car_logic.CarStats_.WheelFL_.IsInContactSmooth_, Position = car_logic.CarStats_.WheelFL_.hubTrans_.localPosition.y, Suspension = CalcSuspension(car_logic.CarStats_.WheelFL_) };
            data.TireFR = new Tire { Contact = car_logic.CarStats_.WheelFR_.IsInContactSmooth_, Position = car_logic.CarStats_.WheelFR_.hubTrans_.localPosition.y, Suspension = CalcSuspension(car_logic.CarStats_.WheelFR_) };
            data.TireBL = new Tire { Contact = car_logic.CarStats_.wheelBL_.IsInContactSmooth_, Position = car_logic.CarStats_.wheelBL_.hubTrans_.localPosition.y, Suspension = CalcSuspension(car_logic.CarStats_.wheelBL_) };
            data.TireBR = new Tire { Contact = car_logic.CarStats_.WheelBR_.IsInContactSmooth_, Position = car_logic.CarStats_.WheelBR_.hubTrans_.localPosition.y, Suspension = CalcSuspension(car_logic.CarStats_.WheelBR_) };


            udp.Send(data);

            _packetId++;

            float CalcSuspension(NitronicCarWheel wheel)
            {
                var pos = Math.Abs(wheel.hubTrans_.localPosition.y);
                var suspension = wheel.SuspensionDistance_;


                var frac = pos / suspension;

                var s = TelemetryLibrary.Maths.EnsureMapRange(pos, 0, suspension, 1, -1);

                return (float)s;

            }

        }
        private void OnEnable()
        {
            Logger.LogInfo("OnEnable...");
            StaticEvent<LocalCarHitFinish.Data>.Subscribe(new StaticEvent<LocalCarHitFinish.Data>.Delegate(RaceEnded));
            StaticEvent<Go.Data>.Subscribe(new StaticEvent<Go.Data>.Delegate(RaceStarted));
            StaticEvent<PauseToggled.Data>.Subscribe(new StaticEvent<PauseToggled.Data>.Delegate(OnGamePaused));
        }

        private void OnDisable()
        {
            Logger.LogInfo("OnDisable...");
            StaticEvent<LocalCarHitFinish.Data>.Unsubscribe(new StaticEvent<LocalCarHitFinish.Data>.Delegate(RaceEnded));
            StaticEvent<Go.Data>.Unsubscribe(new StaticEvent<Go.Data>.Delegate(RaceStarted));
            StaticEvent<PauseToggled.Data>.Unsubscribe(new StaticEvent<PauseToggled.Data>.Delegate(OnGamePaused));

            UnSubscribeFromEvents();

            Logger.LogInfo("Disposing network");
            udp?.Dispose();
        }

        private void SubscribeToEvents()
        {
            Logger.LogInfo("Subscribing to events...");

            playerEvents = car.playerDataLocal_.Events_;
            //playerEvents.Subscribe(new InstancedEvent<TrickComplete.Data>.Delegate(LocalVehicle_TrickComplete));
            //playerEvents.Subscribe(new InstancedEvent<Split.Data>.Delegate(LocalVehicle_Split));
            //playerEvents.Subscribe(new InstancedEvent<CheckpointHit.Data>.Delegate(LocalVehicle_CheckpointPassed));
            //playerEvents.Subscribe(new InstancedEvent<Impact.Data>.Delegate(LocalVehicle_Collided));
            //playerEvents.Subscribe(new InstancedEvent<Death.Data>.Delegate(LocalVehicle_Destroyed));
            //playerEvents.Subscribe(new InstancedEvent<Jump.Data>.Delegate(LocalVehicle_Jumped));
            playerEvents.Subscribe(new InstancedEvent<CarRespawn.Data>.Delegate(LocalVehicle_Respawn));
            //playerEvents.Subscribe(new InstancedEvent<Events.Player.Finished.Data>.Delegate(LocalVehicle_Finished));
            //playerEvents.Subscribe(new InstancedEvent<Explode.Data>.Delegate(LocalVehicle_Exploded));
            //playerEvents.Subscribe(new InstancedEvent<Horn.Data>.Delegate(LocalVehicle_Honked));
        }

        private void UnSubscribeFromEvents()
        {
            Logger.LogInfo("UnSubscribing to events...");
            //playerEvents.Unsubscribe(new InstancedEvent<TrickComplete.Data>.Delegate(LocalVehicle_TrickComplete));
            //playerEvents.Unsubscribe(new InstancedEvent<Split.Data>.Delegate(LocalVehicle_Split));
            //playerEvents.Unsubscribe(new InstancedEvent<CheckpointHit.Data>.Delegate(LocalVehicle_CheckpointPassed));
            //playerEvents.Unsubscribe(new InstancedEvent<Impact.Data>.Delegate(LocalVehicle_Collided));
            //playerEvents.Unsubscribe(new InstancedEvent<Death.Data>.Delegate(LocalVehicle_Destroyed));
            //playerEvents.Unsubscribe(new InstancedEvent<Jump.Data>.Delegate(LocalVehicle_Jumped));
            playerEvents.Unsubscribe(new InstancedEvent<CarRespawn.Data>.Delegate(LocalVehicle_Respawn));
            //playerEvents.Unsubscribe(new InstancedEvent<Events.Player.Finished.Data>.Delegate(LocalVehicle_Finished));
            //playerEvents.Unsubscribe(new InstancedEvent<Explode.Data>.Delegate(LocalVehicle_Exploded));
            //playerEvents.Unsubscribe(new InstancedEvent<Horn.Data>.Delegate(LocalVehicle_Honked));
        }

        private void OnGamePaused(PauseToggled.Data eventData)
        {
            Logger.LogInfo("OnGamePaused..." + eventData.paused_);
            
            paused = eventData.paused_;
        }
        Boolean raceStarted = false;

        private void RaceStarted(Go.Data eventData)
        {
            Log.LogInfo("[Telemetry] RaceStarted...");
            //race_id = Guid.NewGuid();
            //if (sw.IsRunning)
            //{
            //    sw.Stop();
            //}
            //sw = Stopwatch.StartNew();
            raceStarted = true;
            //data = new Dictionary<string, object>
            //{
            //    ["Level"] = G.Sys.GameManager_.LevelName_,
            //    ["Mode"] = G.Sys.GameManager_.ModeName_,
            //    ["Real Time"] = DateTime.Now,
            //    ["Event"] = "start",
            //    ["Time"] = sw.Elapsed.TotalSeconds
            //};
            //Callback(data);
        }

        private void RaceEnded(LocalCarHitFinish.Data eventData)
        {
            Log.LogInfo("{Telemetry] RaceEnded ...");
            //data = new Dictionary<string, object>
            //{
            //    ["Level"] = G.Sys.GameManager_.LevelName_,
            //    ["Mode"] = G.Sys.GameManager_.ModeName_,
            //    ["Real Time"] = DateTime.Now,
            //    ["Event"] = "end",
            //    ["Time"] = sw.Elapsed.TotalSeconds
            //};
            //sw.Stop();
            raceStarted = false;
            //Callback(data);
        }

        private void LocalVehicle_Respawn(CarRespawn.Data eventData)
        {
            Log.LogDebug("LocalVehicle_Respawn");
            Log.LogDebug("EU: " + eventData.rotation_.eulerAngles.ToString());
            //data = new Dictionary<string, object>
            //{
            //    ["Level"] = G.Sys.GameManager_.LevelName_,
            //    ["Mode"] = G.Sys.GameManager_.ModeName_,
            //    ["Real Time"] = DateTime.Now,
            //    ["Time"] = sw.Elapsed.TotalSeconds,
            //    ["Event"] = "respawn"
            //};

            data.Yaw = eventData.rotation_.eulerAngles.y;
            data.Pitch = eventData.rotation_.eulerAngles.x;
            data.Roll = eventData.rotation_.eulerAngles.z;

            //Dictionary<string, object> position = new Dictionary<string, object>
            //{
            //    ["X"] = eventData.position_.x,
            //    ["Y"] = eventData.position_.y,
            //    ["Z"] = eventData.position_.z
            //};
            //Dictionary<string, object> rotation = new Dictionary<string, object>
            //{
            //    ["Pitch"] = eventData.rotation_.eulerAngles.x,
            //    ["Roll"] = eventData.rotation_.eulerAngles.z,
            //    ["Yaw"] = eventData.rotation_.eulerAngles.y
            //};

        }


        

        
    }
}