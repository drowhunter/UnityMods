using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
