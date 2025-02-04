using System;

using UnityEngine;

namespace com.drowmods.DistanceTelemetryMod
{
    public static class QuatMath
    {
        public static Vector3 up(this Quaternion q) => q * Vector3.up;

        public static Vector3 forward(this Quaternion q) => q * Vector3.forward;

        public static Vector3 right(this Quaternion q) => q * Vector3.right;

        public static Vector3 ToPitchYawRoll1(Quaternion q)
        {
            var yaw = (float)Math.Atan2(2 * (q.y * q.w - q.x * q.z), 1 - 2 * (q.y * q.y + q.z * q.z)) ;
            var pitch = (float)Math.Atan2(2 * (q.x * q.w - q.y * q.z), 1 - 2 * (q.x * q.x + q.z * q.z));
            var roll = (float)Math.Asin(2 * (q.x * q.y + q.z * q.w)) ;

            return new Vector3(pitch, yaw, -roll) * 57.29578f;
        }

        private static float MeasureRoll(Transform transform)
        {
            var rotation = transform.rotation;

            var qFwd = transform.forward.ToQuaternion();
            var unfwd = Quaternion.Inverse(qFwd);

            var fwdAlignedRot = (rotation * unfwd).Normalized();

            return fwdAlignedRot.eulerAngles.z;
        }

        public static Vector3 GetYawPitchRollok(Quaternion rotation)
        {
            Vector3 forward = rotation.forward();
            Vector3 upwards = rotation.up();

            float yaw = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
            float pitch = Mathf.Asin(forward.y) * Mathf.Rad2Deg;
            float roll = Mathf.Atan2(upwards.y, upwards.z) * Mathf.Rad2Deg;

            return new Vector3(yaw, pitch, roll);
        }

        public static Vector3 GetPitchYawRollGhetto(Transform transform)
        {
            var data = new Vector3();

            var mainVector = transform.up;


            //var fxz = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            //var u_xy = Vector3.ProjectOnPlane(mainVector, Vector3.forward);
            var u_xy = new Vector3(mainVector.x, mainVector.y, 0);
            float w_p_angle = Vector3.Angle(u_xy, mainVector);


            var u_yz = new Vector3(0, mainVector.y, mainVector.z);
            float w_r_angle = Vector3.Angle(u_yz, transform.right);

            //int rollSign = Vector3.Cross(transform.right, u_yz).y >= 0 ? -1 : 1;

            var dependantVector = new Vector3(transform.forward.x, 0, transform.forward.z);
            var f_dp   = Mathf.Clamp(Vector3.Dot(dependantVector, Vector3.forward), -1, 1);
            float r_dp = Mathf.Clamp(Vector3.Dot(dependantVector, Vector3.right), -1, 1);
            // Vector3.ProjectOnPlane(transform.forward, Vector3.up);

            

            float l_p_angle = (w_p_angle * f_dp) + (w_r_angle * r_dp);
            float l_r_angle = (w_p_angle * r_dp) + (w_r_angle * f_dp);

            var yawAngle = Vector3.Angle(u_xy, Vector3.forward);


            data.y = f_dp; // yawAngle * (Vector3.Cross(Vector3.forward, u_xy).y >= 0 ? -1 : 1);
            data.x = l_p_angle;
            data.z = l_r_angle;

            return data;
        }

        public static Vector3 GetPitchYawRollImpressive(Transform transform)
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

        public static Vector3 GetPitchYawRollImpressive2(Transform transform)
        {
            var unUpRot = Quaternion.Inverse(transform.up.ToQuaternion());

            var upAlignedQ = transform.rotation * unUpRot;

            var upAlignedFwd = Vector3.Normalize(upAlignedQ * Vector3.forward);


            var yaw = Vector3.Angle(upAlignedFwd, Vector3.forward);


            var unRightRot = Quaternion.Inverse(transform.right.ToQuaternion());

            var rightAlignedQ = transform.rotation * unRightRot;

            var rightAlignFwd = Vector3.Normalize(rightAlignedQ * Vector3.forward);

            float pitch = Vector3.Angle(new Vector3(rightAlignFwd.x, 0, rightAlignFwd.z), rightAlignFwd);
            //var pitch = Vector3.Angle(rightAlignFwd, Vector3.forward);


            var unFwdRot = Quaternion.Inverse(transform.forward.ToQuaternion());

            var fwdAlignedQ = transform.rotation * unFwdRot;

            var fwdAlignUp = Vector3.Normalize(fwdAlignedQ * Vector3.up);

            float roll = Vector3.Angle(new Vector3(fwdAlignUp.x, 0, fwdAlignUp.z), fwdAlignUp);
            //var roll = Vector3.Angle(fwdAlignUp, Vector3.up);



            return new Vector3(
                pitch,
                yaw,
                roll
            );
        }

        public static Vector3 StableYawAndRoll(Transform transform)
        {
            // Get the vehicle's rotation as a quaternion
            Quaternion rotation = transform.rotation;

            // Extract the forward and up vectors in local space
            Vector3 localForward = rotation * Vector3.forward;
            Vector3 localUp = rotation * Vector3.up;

            // Calculate yaw (rotation around the global Y-axis)
            float yaw = Mathf.Atan2(localForward.x, localForward.z) * Mathf.Rad2Deg;


            Quaternion stabilizedRotation = Quaternion.Euler(0, yaw, 0);
            //Vector3 stabilizedForward = stabilizedRotation * Vector3.forward;
            //Vector3 stabilizedRight = stabilizedRotation * Vector3.right;

            // Calculate roll (rotation around the local Z-axis)
            Vector3 flatRight = Vector3.Cross(Vector3.up, localForward).normalized;
            float roll = -Mathf.Atan2(Vector3.Dot(localUp, flatRight), Vector3.Dot(localUp, Vector3.up)) * Mathf.Rad2Deg;


            float pitch = Mathf.Asin(localForward.y) * Mathf.Rad2Deg;

            // Log the results
            //Debug.Log("Yaw: " + yaw + ", Roll: " + roll);

            return new Vector3(pitch, yaw, roll);
        }

        public static Vector3 GetPitchYawRoll(Transform transform)
        {
            var data = new Vector3();

            //var fxz = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            var fxz = new Vector3(transform.forward.x, 0, transform.forward.z);

            float yAngle = Vector3.Angle(fxz, transform.forward);


            Vector3 rxz = new Vector3(transform.right.x, 0, transform.right.z);
            float rollAngle = Vector3.Angle(rxz, transform.right);
            int rollSign = Vector3.Cross(transform.right, rxz).y >= 0 ? -1 : 1;


            var yawAngle = Vector3.Angle(fxz, Vector3.forward);


            data.y = yawAngle * (Vector3.Cross(Vector3.forward, fxz).y >= 0 ? -1 : 1);
            data.x = -1f * Math.Sign(transform.forward.y) * yAngle;
            data.z = Math.Sign(transform.right.y) * rollAngle;

            return data;
        }

        public static Vector3 GetPitchYawRollSimple(Transform transform)
        {


            var retval = new Vector3(
                                HalfAngle(transform.eulerAngles.x),
                                HalfAngle(transform.eulerAngles.y),
                                HalfAngle(transform.eulerAngles.z));

            return retval;
        }

        public static Vector3 GetPYRFromQuaternion(Quaternion r)
        {
            float yaw = (float)Math.Atan2(2.0f * (r.y * r.w + r.x * r.z), 1.0f - 2.0f * (r.x * r.x + r.y * r.y));
            float pitch = (float)Math.Asin(2.0f * (r.x * r.w - r.y * r.z));
            float roll = (float)Math.Atan2(2.0f * (r.x * r.y + r.z * r.w), 1.0f - 2.0f * (r.x * r.x + r.z * r.z));

            return new Vector3(pitch, yaw, roll);
        }

        private static float HalfAngle(float angle)
        {
            return angle > 180.0 ? angle - 360f : angle;
        }

        public static Vector3 ToEuler3(this Transform originalTransform)
        {
            var r = originalTransform.localEulerAngles.z;

            var tempGO = new GameObject();
            var t = tempGO.transform;
            t.localRotation = originalTransform.localRotation;

            t.Rotate(0, 0, t.localEulerAngles.z * -1);

            var p = t.localEulerAngles.y;


            t.Rotate(t.localEulerAngles.x * -1, 0, 0);

            GameObject.Destroy(tempGO);
            var y = t.localEulerAngles.y;

            return new Vector3(p, y, r);
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

        public static float CalculatePitch(Vector3 worldUpVector)
        {
            // Assume world up vector is (0, 1, 0)
            var worldUp = Vector3.up;

            // Calculate dot product
            float dotProduct = worldUpVector.y;

            // Calculate pitch
            var pitch = (float)Math.Asin(dotProduct) * 57.29578f;

            return pitch;
        }


        public static float CalculateYaw(Vector3 worldUpVector)
        {
            // Assume world up vector is (0, 1, 0)
            var worldUp = Vector3.up;
            // Calculate dot product
            float dotProduct = worldUpVector.y;
            // Calculate pitch
            var pitch = (float)Math.Asin(dotProduct) * 57.29578f;
            return pitch;
        }


        public static float GetPitch(Quaternion q)
        {
            QuaternionToRotationMatrix(q, out var matrix);

            Vector3 localUp = Vector3.up; // Local up vector
            Vector3 worldUpVector = TransformVector(matrix, localUp);

            return CalculatePitch(worldUpVector);
        }

    
    }

    public static class QuaternionToWorldYawPitchRoll
    {
        public static Matrix4x4 QuaternionToRotationMatrix(Quaternion q)
        {
            var matrix = new Matrix4x4();

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

            return matrix;
        }

        public static Vector3 TransformVector(Matrix4x4 matrix, Vector3 vector)
        {
            var result = new Vector3();
            result.x = matrix[0, 0] * vector[0] + matrix[0, 1] * vector[1] + matrix[0, 2] * vector[2];
            result.y = matrix[1, 0] * vector[0] + matrix[1, 1] * vector[1] + matrix[1, 2] * vector[2];
            result.z = matrix[2, 0] * vector[0] + matrix[2, 1] * vector[1] + matrix[2, 2] * vector[2];

            return result;
        }

        public static float CalculateAngle(Vector3 vector, Vector3 referenceVector)
        {
            var dotProduct = Vector3.Dot(vector, referenceVector);             

            return (float) Math.Acos(dotProduct / (vector.magnitude * referenceVector.magnitude)) * 57.29578f;
        }

        public static Vector3 Doit(Quaternion q)
        {
            
            var matrix = QuaternionToRotationMatrix(q);

            // Local vectors
            var localUp = Vector3.up;
            var localForward = Vector3.forward;
            var localRight = Vector3.right;

            // Transform to world space            
            var worldUp = TransformVector(matrix, localUp);
            var worldForward = TransformVector(matrix, localForward);
            var worldRight = TransformVector(matrix, localRight);

            // World reference vectors
            var worldUpRef = Vector3.up;
            var worldForwardRef = Vector3.forward;
            var worldRightRef = Vector3.right;

            // Calculate angles
            var pitch = CalculateAngle(worldUp, worldUpRef);
            var yaw = CalculateAngle(worldForward, worldForwardRef);
            var roll = CalculateAngle(worldRight, worldRightRef);

            return new Vector3(pitch, yaw, roll);
        }

    }

}