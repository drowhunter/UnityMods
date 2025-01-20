using System;

using UnityEngine;

namespace com.drowmods.DistanceTelemetryMod
{
    public static class QuatMath
    {

        public static Vector3 GetPitchYawRoll(Transform transform)
        {
            var data = new Vector3();

            float pitchAngle = Vector3.Angle(new Vector3(transform.forward.x, 0, transform.forward.z), transform.forward);

            float rollAngle = Vector3.Angle(new Vector3(transform.right.x, 0, transform.right.z), transform.right);

            var yawAngle = Vector3.Angle(transform.forward, Vector3.forward);

            data.y = yawAngle * Mathf.Sign(transform.forward.x);
            data.x = -1f * Math.Sign(transform.forward.y) * pitchAngle;
            data.z = Math.Sign(transform.right.y) * rollAngle;

            return data;
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
        
    }

    
}