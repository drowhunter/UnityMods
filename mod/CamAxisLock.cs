using HarmonyLib;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Threading;

namespace com.drowmods.depth3dunhinged
{
    [HarmonyPatch(typeof(CamAxisLock))]
    internal class CamAxisLockPatches 
    {
        static Mutex mutex;
        static MemoryMappedFile mmf;
        static MemoryMappedViewAccessor accessor;
        static int _packetId;

        [HarmonyPostfix]
        [HarmonyPatch(nameof(CamAxisLock.Start))]
        public static void Start_Postfix(CamAxisLock __instance)
        {
            mutex = new Mutex(false, "Global\\CamAxisLock");
            mmf = MemoryMappedFile.CreateOrOpen("FPGeneric", Marshal.SizeOf(typeof(FreePieIO6Dof)));
            
            accessor = mmf.CreateViewAccessor();
            _packetId = 0;

        }


        [HarmonyPostfix]
        [HarmonyPatch(nameof(CamAxisLock.Update))]
        public static void Update_Postfix(CamAxisLock __instance, ref object[] __state)
        {
            var cam = __instance.tr_cam;

            var data = new FreePieIO6Dof
            {
                DataId = _packetId,
                X = cam.position.x,
                Y = cam.position.y,
                Z = cam.position.z,
                Yaw = hemiCircle(cam.transform.localEulerAngles.y),
                Pitch = hemiCircle(cam.transform.localEulerAngles.x),
                Roll = -hemiCircle(cam.transform.localEulerAngles.z)
            };

            accessor.Write(0, ref data);
                
            _packetId++;
        }

        private static float hemiCircle(float angle)
        {
            return angle >= 180 ? angle - 360 : angle;
        }



        [HarmonyCleanup]
        public static void CleanUp()
        {
            FileLog.Log("Cleaning Up");           

            mmf?.Dispose();

            accessor?.Dispose();
        }

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct FreePieIO6Dof
    {
        public int DataId;

        public float Yaw;
        public float Pitch;
        public float Roll;

        public float X;
        public float Y;
        public float Z;
    }
}
