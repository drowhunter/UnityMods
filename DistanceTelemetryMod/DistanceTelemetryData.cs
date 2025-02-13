﻿using System.Runtime.InteropServices;
using UnityEngine;

namespace com.drowmods.DistanceTelemetryMod
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct DistanceTelemetryData
    {
        public bool GamePaused;        
        public bool IsRacing;
        public float KPH;

        public Vector3 Rotation;

        public Vector3 AngularVelocity;

        public float cForce;

        public Vector3 Velocity;        
        public Vector3 Accel;

        public bool Boost;
        public bool Grip;
        public bool WingsOpen;

        public bool IsCarEnabled;
        public bool IsCarIsActive;
        public bool IsCarDestroyed;
        public bool AllWheelsOnGround;           
        public bool IsGrav;

        public float TireFL;
        public float TireFR;
        public float TireBL;
        public float TireBR;

        public Quaternion Orientation;

    }

    
}
