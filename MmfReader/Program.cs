using com.drowmods.DistanceTelemetryMod;

using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace MmfReader
{
    internal class Program
    {
        static CancellationTokenSource cts = new CancellationTokenSource();

        static void Main(string[] args)
        {
            new Thread(static () =>
            {
                Console.WriteLine("Start Read Thread");                

                while (!cts.Token.IsCancellationRequested)
                {
#pragma warning disable CA1416 // Validate platform compatibility
                //using (var mmf = MemoryMappedFile.CreateOrOpen("FPGeneric", Marshal.SizeOf(typeof(FreePieIO6Dof))))
                //{
                //    using (var accessor = mmf.CreateViewAccessor())
                //    {
                //        var freepie6dof = new FreePieIO6Dof();
                //        accessor.Read(0, out freepie6dof);

                //        for (var i = 1; i < 4; i++)
                //        {
                //            Console.SetCursorPosition(0, i);
                //            ClearCurrentConsoleLine();
                //        }
                //        Console.WriteLine($"Yaw: {freepie6dof.Yaw:F4} Pitch: {freepie6dof.Pitch:F4} Roll: {freepie6dof.Roll:F4}");
                //        Console.WriteLine($"x: {freepie6dof.X:F4}  y:  {freepie6dof.Y:F4} z: {freepie6dof.Z:F4}");
                //    }

                //}

                using (var mmf = MemoryMappedFile.CreateOrOpen("FPGeneric", Marshal.SizeOf(typeof(DistanceTelemetryData))))
                    {
                        using (var accessor = mmf.CreateViewAccessor())
                        {
                            var data = new DistanceTelemetryData();
                            accessor.Read(0, out data);

                            for (var i = 1; i < 4; i++)
                            {
                                Console.SetCursorPosition(0, i);
                                ClearCurrentConsoleLine();
                            }
                            Console.WriteLine($"Yaw: {data.Yaw:F4} Pitch: {data.Pitch:F4} Roll: {data.Roll:F4}");
                            Console.WriteLine($"x: {data.X:F4}  y:  {data.Y:F4} z: {data.Z:F4}");
                        }

                    }
#pragma warning restore CA1416 // Validate platform compatibility


                    Thread.Sleep(16);                 
                    

                }
            }).Start();

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
            cts.Cancel();
            Console.WriteLine("Quitting ..");
            Console.ReadKey();
        }

        public static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
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
