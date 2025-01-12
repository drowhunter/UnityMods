using System;

namespace TelemetryLibrary
{
    internal class Maths
    {
        public const float PI = (float)Math.PI;

        public const float DEG_2_RAD = (float)Math.PI / 180f;

        public const float RAD_2_DEG = 57.29578f;

        public static float HemiCircle(float angle)
        {
            return angle >= 180 ? angle - 360 : angle;
        }

        public static float ReverseHemiCircle(float angle)
        {
            return angle < 0 ? 360 + angle : angle;
        }

        //public float CalculateCentripetalAcceleration(Vector3 velocity, Vector3 angularVelocity)
        //{
        //    var Fc = velocity.Length() * angularVelocity.Length();

        //    return Fc * (angularVelocity.Y >= 0 ? -1 : 1);

        //}
        public static double MapRange(double x, double xMin, double xMax, double yMin, double yMax)
        {
            return yMin + (yMax - yMin) * (x - xMin) / (xMax - xMin);
        }

        public static double EnsureMapRange(double x, double xMin, double xMax, double yMin, double yMax)
        {
            return Math.Max(Math.Min(MapRange(x, xMin, xMax, yMin, yMax), Math.Max(yMin, yMax)), Math.Min(yMin, yMax));
        }

        public static PitchYawRoll QuatToPitchYawRoll(float w, float x, float y, float z)
        {
            var yaw = (float) Math.Atan2(2 * y * w - 2 * x * z, 1 - 2 * y * y - 2 * z * z) * RAD_2_DEG;
            var pitch = (float) Math.Atan2(2 * x * w - 2 * y * z, 1 - 2 * x * x - 2 * z * z) * RAD_2_DEG;
            var roll = (float) Math.Asin(2 * x * y + 2 * z * w) * RAD_2_DEG;

            return new PitchYawRoll(pitch, yaw, roll);
        }
        

        
    }
    internal struct PitchYawRoll
    {
        public float pitch;
        public float yaw;
        public float roll;

        public PitchYawRoll(float pitch, float yaw, float roll)
        {
            this.pitch = pitch;
            this.yaw = yaw;
            this.roll = roll;
        }
    }
}
