using System;

using UnityEngine;

namespace com.drowmods.DistanceTelemetryMod
{
    public static class QuatMath
    {
        public static Vector3 GetEulerAngles(Transform transform)
        {
            var data = new Vector3();

            var yaw = Vector3.Angle(transform.forward, Vector3.forward);

            float pitch = Vector3.Angle(new Vector3(transform.forward.x, 0, transform.forward.z), transform.forward);

            float roll = Vector3.Angle(new Vector3(transform.right.x, 0, transform.right.z), transform.right);

            

            return new Vector3(
                data.x = Mathf.Sign(transform.forward.y) * pitch,
                data.y =  yaw,//Mathf.Sign(transform.forward.x) *
                data.z = Mathf.Sign(transform.right.y) * roll
            );
        }


        private static float HalfAngle(float angle)
        {
            return angle > 180.0 ? angle - 360f : angle;
        }

        

    
        public static void QuaternionToRotationMatrix(Quaternion q, out Matrix4x4 matrix)
        {
            matrix = new Matrix4x4();

            float ww = q.w * q.w;
            float xx = q.x * q.x;
            float yy = q.y * q.y;
            float zz = q.z * q.z;

            matrix[0, 0] = ww + xx - yy - zz;
            matrix[0, 1] = 2 * (q.x * q.y - q.w * q.z);
            matrix[0, 2] = 2 * (q.x * q.z + q.w * q.y);

            matrix[1, 0] = 2 * (q.x * q.y + q.w * q.z);
            matrix[1, 1] = ww - xx + yy - zz;
            matrix[1, 2] = 2 * (q.y * q.z - q.w * q.x);

            matrix[2, 0] = 2 * (q.x * q.z - q.w * q.y);
            matrix[2, 1] = 2 * (q.y * q.z + q.w * q.x);
            matrix[2, 2] = ww - xx - yy + zz;
        }

        public static Vector3 TransformVector(Matrix4x4 matrix, Vector3 vector)
        {
            var result = new Vector3();
            result.x = matrix[0, 0] * vector[0] + matrix[0, 1] * vector[1] + matrix[0, 2] * vector[2];
            result.y = matrix[1, 0] * vector[0] + matrix[1, 1] * vector[1] + matrix[1, 2] * vector[2];
            result.z = matrix[2, 0] * vector[0] + matrix[2, 1] * vector[1] + matrix[2, 2] * vector[2];

            return result;
        }

        

    
    }

    

}