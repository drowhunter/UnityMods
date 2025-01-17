using System;

using UnityEngine;

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

        public static PitchYawRoll ToPitchYawRoll(float w, float x, float y, float z)
        {
            var yaw = Math.Atan2(2 * (y * w - x * z), 1 - 2 * (y * y + z * z)) * RAD_2_DEG;
            var pitch = Math.Atan2(2 * (x * w - y * z), 1 - 2 * (x * x + z * z)) * RAD_2_DEG;
            var roll = Math.Asin(2 * (x * y + z * w)) * RAD_2_DEG;

            return new PitchYawRoll((float)pitch, (float)yaw, (float)-roll);
        }

        PitchYawRoll ToEulerSmooth(float w, float x, float y, float z)
        {
            var yaw = Math.Atan2(2 * (y * w - x * z), 1 - 2 * (y * y + z * z)) * RAD_2_DEG;
            var pitch = Math.Atan2(2 * (x * w - y * z), 1 - 2 * (x * x + z * z)) * RAD_2_DEG;
            var roll = Math.Asin(2 * (x * y + z * w)) * RAD_2_DEG;

            pitch = LimitAngle(pitch, 90, 20);
            roll = LimitAngle(roll, 90, 20);

            return new PitchYawRoll((float)pitch, (float)yaw, (float)-roll);
        }

        /// <summary>
        /// Limit angle to a maximum value 
        /// </summary>
        /// <param name="degrees"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static double LimitAngle(double degrees, float inputRange, float max)
        {
            double v = 0;
            if (Math.Abs(degrees) <= inputRange)
            {
                v = degrees;
            }
            else
            {
                v = (180 - Math.Abs(degrees)) * (degrees < 0 ? -1 : 1);
            }


            return EnsureMapRange(v, -inputRange, inputRange, -max, max);
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
