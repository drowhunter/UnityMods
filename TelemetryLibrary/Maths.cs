using System;
using System.Collections.Generic;
using System.Text;

//using UnityEngine;

namespace TelemetryLibrary
{
    internal class Maths
    {
        public static float HemiCircle(float angle)
        {
            return angle >= 180 ? angle - 360 : angle;
        }

        public static float ReverseHemiCircle(float angle)
        {
            return angle < 0 ? 360 - angle : angle;
        }

        //public float CalculateCentripetalAcceleration(Vector3 velocity, Vector3 angularVelocity)
        //{
        //    var Fc = velocity.Length() * angularVelocity.Length();

        //    return Fc * (angularVelocity.Y >= 0 ? -1 : 1);

        //}
    }
}
