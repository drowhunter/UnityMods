using System;

using UnityEngine;

namespace com.drowmods.DistanceTelemetryMod
{
    public static class Maths
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