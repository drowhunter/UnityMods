
using System;
using System.Diagnostics;
namespace TelemetryLibrary
{
    [DebuggerDisplay("w:{w}, x:{x}, y:{y}, z:{z}")]
    public struct Quat
    {
        public float w, x, y, z;

        public Quat(float w, float x, float y, float z)
        {
            this.w = w;
            this.x = x;
            this.y = y;
            this.z = z;            
        }

        public static Quat FromEuler(float p, float y, float r)
        {
            // Convert degrees to radians
            p = p * (float)Math.PI / 180.0f;
            y = y * (float)Math.PI / 180.0f;
            r = r * (float)Math.PI / 180.0f;

            float c1 = (float)Math.Cos(y / 2);
            float c2 = (float)Math.Cos(r / 2);
            float c3 = (float)Math.Cos(p / 2);
            float s1 = (float)Math.Sin(y / 2);
            float s2 = (float)Math.Sin(r / 2);
            float s3 = (float)Math.Sin(p / 2);

            Quat q = new Quat();
            q.w = c1 * c2 * c3 - s1 * s2 * s3;
            q.x = s1 * s2 * c3 + c1 * c2 * s3;
            q.y = s1 * c2 * c3 + c1 * s2 * s3;
            q.z = c1 * s2 * c3 - s1 * c2 * s3;

            return q;
        }
    }

    [DebuggerDisplay("p:{p}, y:{y}, r:{r}")]
    public struct Euler
    {

        public float p, y, r;

        public Euler(float p, float y, float r)
        {
            this.p = p;
            this.y = y;
            this.r = r;
        }
    }



    public class QuaternionConverter
    {
        

        const float rad2deg = 180.0f / (float)Math.PI;


        #region Intrinsic Rotation
        public static Euler ToEulerXYZ(Quat q)
        {
            float t0 = 2.0f * (q.w * q.x + q.y * q.z);
            float t1 = 1.0f - 2.0f * (q.x * q.x + q.y * q.y);
            float t2 = 2.0f * (q.w * q.y - q.z * q.x);
            float t3 = 2.0f * (q.w * q.z + q.x * q.y);
            float t4 = 1.0f - 2.0f * (q.y * q.y + q.z * q.z);

            float EulerX = (float)Math.Atan2(t0, t1);
            float EulerY = (float)Math.Asin(t2);
            float EulerZ = (float)Math.Atan2(t3, t4);

            return new Euler(EulerX * rad2deg, EulerY * rad2deg, EulerZ * rad2deg);
        }

        public static Euler ToEulerXZY(Quat q)
        {
            float t0 = 2.0f * (q.w * q.z + q.x * q.y);
            float t1 = 1.0f - 2.0f * (q.y * q.y + q.z * q.z);
            float t2 = 2.0f * (q.w * q.x - q.y * q.z);
            float t3 = 2.0f * (q.w * q.y + q.x * q.z);
            float t4 = 1.0f - 2.0f * (q.x * q.x + q.z * q.z);

            float EulerX = (float)Math.Atan2(t0, t1);
            float EulerY = (float)Math.Atan2(t3, t4);
            float EulerZ = (float)Math.Asin(t2);

            return new Euler(EulerX * rad2deg, EulerY * rad2deg, EulerZ * rad2deg);
        }

        public static Euler ToEulerYZX(Quat q)
        {
            float t0 = 2.0f * (q.w * q.y - q.x * q.z);
            float t1 = 2.0f * (q.w * q.z + q.x * q.y);
            float t2 = 1.0f - 2.0f * (q.y * q.y + q.z * q.z);
            float t3 = 2.0f * (q.w * q.x + q.y * q.z);
            float t4 = 1.0f - 2.0f * (q.x * q.x + q.y * q.y);

            float EulerX = (float)Math.Atan2(t1, t2);
            float EulerY = (float)Math.Asin(t0);
            float EulerZ = (float)Math.Atan2(t3, t4);

            return new Euler(EulerX * rad2deg, EulerY * rad2deg, EulerZ * rad2deg);
        }

        public static Euler ToEulerYXZ(Quat q)
        {
            float t0 = 2.0f * (q.w * q.x - q.y * q.z);
            float t1 = 2.0f * (q.w * q.y + q.x * q.z);
            float t2 = 1.0f - 2.0f * (q.x * q.x + q.z * q.z);
            float t3 = 2.0f * (q.w * q.z + q.y * q.x);
            float t4 = 1.0f - 2.0f * (q.x * q.x + q.y * q.y);

            float EulerX = (float)Math.Asin(t1);
            float EulerY = (float)Math.Atan2(t0, t4);
            float EulerZ = (float)Math.Atan2(t3, t2);

            return new Euler(EulerX * rad2deg, EulerY * rad2deg, EulerZ * rad2deg);
        }

        public static Euler ToEulerZXY(Quat q)
        {
            float t0 = 2.0f * (q.w * q.y + q.x * q.z);
            float t1 = 1.0f - 2.0f * (q.y * q.y + q.z * q.z);
            float t2 = 2.0f * (q.w * q.z - q.x * q.y);
            float t3 = 2.0f * (q.w * q.x + q.y * q.z);
            float t4 = 1.0f - 2.0f * (q.x * q.x + q.z * q.z);

            float EulerX = (float)Math.Asin(t2);
            float EulerY = (float)Math.Atan2(t3, t4);
            float EulerZ = (float)Math.Atan2(t0, t1);

            return new Euler(EulerX * rad2deg, EulerY * rad2deg, EulerZ * rad2deg);
        }

        public static Euler ToEulerZYX(Quat q)
        {
            float t0 = 2.0f * (q.w * q.z + q.x * q.y);
            float t1 = 1.0f - 2.0f * (q.z * q.z + q.y * q.y);
            float t2 = 2.0f * (q.w * q.y - q.z * q.x);
            float t3 = 1.0f - 2.0f * (q.x * q.x + q.y * q.y);
            float t4 = 1.0f - 2.0f * (q.z * q.z + q.x * q.x);

            float EulerX = (float)Math.Atan2(t0, t1);
            float EulerY = (float)Math.Asin(t2);
            float EulerZ = (float)Math.Atan2(t3, t4);

            return new Euler(EulerX * rad2deg, EulerY * rad2deg, EulerZ * rad2deg);
        }

        #endregion

        #region Extrinsic Rotation

        // ZXZ
        public static Euler ToEulerZXZ(Quat q)
        {
            float t0 = 2.0f * (q.w * q.x - q.y * q.z);
            float t1 = 2.0f * (q.w * q.y + q.x * q.z);
            float t2 = 1.0f - 2.0f * (q.x * q.x + q.y * q.y);
            float t3 = 2.0f * (q.w * q.z + q.x * q.y);
            float t4 = 1.0f - 2.0f * (q.y * q.y + q.z * q.z);

            float EulerX = (float)Math.Atan2(t0, t1);
            float EulerY = (float)Math.Acos(t2);
            float EulerZ = (float)Math.Atan2(t3, t4);

            return new Euler(EulerX * rad2deg, EulerY * rad2deg, EulerZ * rad2deg);
        }

        // XYX
        public static Euler ToEulerXYX(Quat q)
        {
            float t0 = 2.0f * (q.w * q.x + q.y * q.z);
            float t1 = 1.0f - 2.0f * (q.x * q.x + q.z * q.z);
            float t2 = 2.0f * (q.w * q.y - q.z * q.x);
            float t3 = 2.0f * (q.w * q.z + q.x * q.y);
            float t4 = 1.0f - 2.0f * (q.y * q.y + q.z * q.z);

            float EulerX = (float)Math.Atan2(t3, t4);
            float EulerY = (float)Math.Acos(t1);
            float EulerZ = (float)Math.Atan2(t0, t2);

            return new Euler(EulerX * rad2deg, EulerY * rad2deg, EulerZ * rad2deg);
        }

        // YZY
        public static Euler ToEulerYZY(Quat q)
        {
            float t0 = 2.0f * (q.w * q.y + q.x * q.z);
            float t1 = 1.0f - 2.0f * (q.x * q.x + q.z * q.z);
            float t2 = 2.0f * (q.w * q.z - q.x * q.y);
            float t3 = 2.0f * (q.w * q.x + q.y * q.z);
            float t4 = 1.0f - 2.0f * (q.y * q.y + q.z * q.z);

            float EulerX = (float)Math.Atan2(t3, t4);
            float EulerY = (float)Math.Acos(t1);
            float EulerZ = (float)Math.Atan2(t0, t2);

            return new Euler(EulerX * rad2deg, EulerY * rad2deg, EulerZ * rad2deg);
        }

        // XZX
        public static Euler ToEulerXZX(Quat q)
        {
            float t0 = 2.0f * (q.w * q.x - q.y * q.z);
            float t1 = 2.0f * (q.w * q.y + q.x * q.z);
            float t2 = 1.0f - 2.0f * (q.x * q.x + q.y * q.y);
            float t3 = 2.0f * (q.w * q.z + q.x * q.y);
            float t4 = 1.0f - 2.0f * (q.y * q.y + q.z * q.z);

            float EulerX = (float)Math.Atan2(t3, t4);
            float EulerY = (float)Math.Acos(t2);
            float EulerZ = (float)Math.Atan2(t0, t1);

            return new Euler(EulerX * rad2deg, EulerY * rad2deg, EulerZ * rad2deg);
        }

        // YXY
        public static Euler ToEulerYXY(Quat q)
        {
            float t0 = 2.0f * (q.w * q.y + q.x * q.z);
            float t1 = 1.0f - 2.0f * (q.x * q.x + q.z * q.z);
            float t2 = 2.0f * (q.w * q.x - q.z * q.y);
            float t3 = 2.0f * (q.w * q.z + q.x * q.y);
            float t4 = 1.0f - 2.0f * (q.y * q.y + q.z * q.z);

            float EulerX = (float)Math.Atan2(t0, t1);
            float EulerY = (float)Math.Acos(t4);
            float EulerZ = (float)Math.Atan2(t2, t3);

            return new Euler(EulerX * rad2deg, EulerY * rad2deg, EulerZ * rad2deg);
        }

        // ZYZ
        public static Euler ToEulerZYZ(Quat q)
        {
            float t0 = 2.0f * (q.w * q.z - q.x * q.y);
            float t1 = 1.0f - 2.0f * (q.x * q.x + q.y * q.y);
            float t2 = 2.0f * (q.w * q.x + q.y * q.z);
            float t3 = 2.0f * (q.w * q.y - q.x * q.z);
            float t4 = 1.0f - 2.0f * (q.y * q.y + q.z * q.z);

            float EulerX = (float)Math.Atan2(t2, t3);
            float EulerY = (float)Math.Acos(t1);
            float EulerZ = (float)Math.Atan2(t0, t4);

            return new Euler(EulerX * rad2deg, EulerY * rad2deg, EulerZ * rad2deg);
        }

        #endregion
    }
}