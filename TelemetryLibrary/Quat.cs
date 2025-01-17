
using System;
namespace TelemetryLibrary
{
    public class Quat
    {
        public double w, x, y, z;

        public Quat(double w, double x, double y, double z)
        {
            this.w = w;
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    public class Euler
    {
        public double x, y, z;

        public Euler(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    public class QuaternionConverter
    {
        #region Intrinsic Rotation
        public static Euler ToEulerXYZ(Quat q)
        {
            double t0 = 2.0 * (q.w * q.x + q.y * q.z);
            double t1 = 1.0 - 2.0 * (q.x * q.x + q.y * q.y);
            double t2 = 2.0 * (q.w * q.y - q.z * q.x);
            double t3 = 2.0 * (q.w * q.z + q.x * q.y);
            double t4 = 1.0 - 2.0 * (q.y * q.y + q.z * q.z);

            double EulerX = Math.Atan2(t0, t1);
            double EulerY = Math.Asin(t2);
            double EulerZ = Math.Atan2(t3, t4);

            return new Euler(EulerX, EulerY, EulerZ);
        }

        public static Euler ToEulerXZY(Quat q)
        {
            double t0 = 2.0 * (q.w * q.z + q.x * q.y);
            double t1 = 1.0 - 2.0 * (q.y * q.y + q.z * q.z);
            double t2 = 2.0 * (q.w * q.x - q.y * q.z);
            double t3 = 2.0 * (q.w * q.y + q.x * q.z);
            double t4 = 1.0 - 2.0 * (q.x * q.x + q.z * q.z);

            double EulerX = Math.Atan2(t0, t1);
            double EulerY = Math.Atan2(t3, t4);
            double EulerZ = Math.Asin(t2);

            return new Euler(EulerX, EulerY, EulerZ);
        }

        public static Euler ToEulerYZX(Quat q)
        {
            double t0 = 2.0 * (q.w * q.y - q.x * q.z);
            double t1 = 2.0 * (q.w * q.z + q.x * q.y);
            double t2 = 1.0 - 2.0 * (q.y * q.y + q.z * q.z);
            double t3 = 2.0 * (q.w * q.x + q.y * q.z);
            double t4 = 1.0 - 2.0 * (q.x * q.x + q.y * q.y);

            double EulerX = Math.Atan2(t1, t2);
            double EulerY = Math.Asin(t0);
            double EulerZ = Math.Atan2(t3, t4);

            return new Euler(EulerX, EulerY, EulerZ);
        }

        public static Euler ToEulerYXZ(Quat q)
        {
            double t0 = 2.0 * (q.w * q.x - q.y * q.z);
            double t1 = 2.0 * (q.w * q.y + q.x * q.z);
            double t2 = 1.0 - 2.0 * (q.x * q.x + q.z * q.z);
            double t3 = 2.0 * (q.w * q.z + q.y * q.x);
            double t4 = 1.0 - 2.0 * (q.x * q.x + q.y * q.y);

            double EulerX = Math.Asin(t1);
            double EulerY = Math.Atan2(t0, t4);
            double EulerZ = Math.Atan2(t3, t2);

            return new Euler(EulerX, EulerY, EulerZ);
        }

        public static Euler ToEulerZXY(Quat q)
        {
            double t0 = 2.0 * (q.w * q.y + q.x * q.z);
            double t1 = 1.0 - 2.0 * (q.y * q.y + q.z * q.z);
            double t2 = 2.0 * (q.w * q.z - q.x * q.y);
            double t3 = 2.0 * (q.w * q.x + q.y * q.z);
            double t4 = 1.0 - 2.0 * (q.x * q.x + q.z * q.z);

            double EulerX = Math.Asin(t2);
            double EulerY = Math.Atan2(t3, t4);
            double EulerZ = Math.Atan2(t0, t1);

            return new Euler(EulerX, EulerY, EulerZ);
        }

        public static Euler ToEulerZYX(Quat q)
        {
            double t0 = 2.0 * (q.w * q.z + q.x * q.y);
            double t1 = 1.0 - 2.0 * (q.z * q.z + q.y * q.y);
            double t2 = 2.0 * (q.w * q.y - q.z * q.x);
            double t3 = 1.0 - 2.0 * (q.x * q.x + q.y * q.y);
            double t4 = 1.0 - 2.0 * (q.z * q.z + q.x * q.x);

            double EulerX = Math.Atan2(t0, t1);
            double EulerY = Math.Asin(t2);
            double EulerZ = Math.Atan2(t3, t4);

            return new Euler(EulerX, EulerY, EulerZ);
        }

        #endregion

        #region Extrinsic Rotation

        // ZXZ
        public static Euler ToEulerZXZ(Quat q)
        {
            double t0 = 2.0 * (q.w * q.x - q.y * q.z);
            double t1 = 2.0 * (q.w * q.y + q.x * q.z);
            double t2 = 1.0 - 2.0 * (q.x * q.x + q.y * q.y);
            double t3 = 2.0 * (q.w * q.z + q.x * q.y);
            double t4 = 1.0 - 2.0 * (q.y * q.y + q.z * q.z);

            double EulerX = Math.Atan2(t0, t1);
            double EulerY = Math.Acos(t2);
            double EulerZ = Math.Atan2(t3, t4);

            return new Euler(EulerX, EulerY, EulerZ);
        }

        // XYX
        public static Euler ToEulerXYX(Quat q)
        {
            double t0 = 2.0 * (q.w * q.x + q.y * q.z);
            double t1 = 1.0 - 2.0 * (q.x * q.x + q.z * q.z);
            double t2 = 2.0 * (q.w * q.y - q.z * q.x);
            double t3 = 2.0 * (q.w * q.z + q.x * q.y);
            double t4 = 1.0 - 2.0 * (q.y * q.y + q.z * q.z);

            double EulerX = Math.Atan2(t3, t4);
            double EulerY = Math.Acos(t1);
            double EulerZ = Math.Atan2(t0, t2);

            return new Euler(EulerX, EulerY, EulerZ);
        }

        // YZY
        public static Euler ToEulerYZY(Quat q)
        {
            double t0 = 2.0 * (q.w * q.y + q.x * q.z);
            double t1 = 1.0 - 2.0 * (q.x * q.x + q.z * q.z);
            double t2 = 2.0 * (q.w * q.z - q.x * q.y);
            double t3 = 2.0 * (q.w * q.x + q.y * q.z);
            double t4 = 1.0 - 2.0 * (q.y * q.y + q.z * q.z);

            double EulerX = Math.Atan2(t3, t4);
            double EulerY = Math.Acos(t1);
            double EulerZ = Math.Atan2(t0, t2);

            return new Euler(EulerX, EulerY, EulerZ);
        }

        // XZX
        public static Euler ToEulerXZX(Quat q)
        {
            double t0 = 2.0 * (q.w * q.x - q.y * q.z);
            double t1 = 2.0 * (q.w * q.y + q.x * q.z);
            double t2 = 1.0 - 2.0 * (q.x * q.x + q.y * q.y);
            double t3 = 2.0 * (q.w * q.z + q.x * q.y);
            double t4 = 1.0 - 2.0 * (q.y * q.y + q.z * q.z);

            double EulerX = Math.Atan2(t3, t4);
            double EulerY = Math.Acos(t2);
            double EulerZ = Math.Atan2(t0, t1);

            return new Euler(EulerX, EulerY, EulerZ);
        }

        // YXY
        public static Euler ToEulerYXY(Quat q)
        {
            double t0 = 2.0 * (q.w * q.y + q.x * q.z);
            double t1 = 1.0 - 2.0 * (q.x * q.x + q.z * q.z);
            double t2 = 2.0 * (q.w * q.x - q.z * q.y);
            double t3 = 2.0 * (q.w * q.z + q.x * q.y);
            double t4 = 1.0 - 2.0 * (q.y * q.y + q.z * q.z);

            double EulerX = Math.Atan2(t0, t1);
            double EulerY = Math.Acos(t4);
            double EulerZ = Math.Atan2(t2, t3);

            return new Euler(EulerX, EulerY, EulerZ);
        }

        // ZYZ
        public static Euler ToEulerZYZ(Quat q)
        {
            double t0 = 2.0 * (q.w * q.z - q.x * q.y);
            double t1 = 1.0 - 2.0 * (q.x * q.x + q.y * q.y);
            double t2 = 2.0 * (q.w * q.x + q.y * q.z);
            double t3 = 2.0 * (q.w * q.y - q.x * q.z);
            double t4 = 1.0 - 2.0 * (q.y * q.y + q.z * q.z);

            double EulerX = Math.Atan2(t2, t3);
            double EulerY = Math.Acos(t1);
            double EulerZ = Math.Atan2(t0, t4);

            return new Euler(EulerX, EulerY, EulerZ);
        }


        #endregion

    }
}