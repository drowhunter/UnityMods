using System.Runtime.InteropServices;

namespace com.drowhunter.DistanceTelemetryMod
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DistanceTelemetryData
    {
        public bool GamePaused;
        public bool IsRacing;
        public float KPH;

        public float Pitch;
        public float Yaw;
        public float Roll;

        public float AngularVelocityX;
        public float AngularVelocityY;
        public float AngularVelocityZ;

        public float cForce;

        public float VelocityX;
        public float VelocityY;
        public float VelocityZ;

        public float AccelX;
        public float AccelY;
        public float AccelZ;

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

        public float OrientationX;
        public float OrientationY;
        public float OrientationZ;
        public float OrientationW;
    }

    
}
