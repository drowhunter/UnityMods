using System.Net;
using System.Runtime.InteropServices;
using TelemetryLibrary;

namespace MmfReader
{
    internal class Program
    {
        static CancellationTokenSource cts = new CancellationTokenSource();

        static void Main(string[] args)
        {
            new Thread(static async () =>
            {
                var udp = new UdpTelemetry<DistanceTelemetryData>(new UdpTelemetryConfig
                {
                    ReceiveAddress = new IPEndPoint(IPAddress.Any, 12345)
                });


                Console.WriteLine("Start Read Thread");
                var ms = new MemoryStream();
                
                while (!cts.Token.IsCancellationRequested)
                {
                    var telem = await udp.ReceiveAsync(cts.Token);

                    for (var i = 1; i < 4; i++)
                    {
                        Console.SetCursorPosition(0, i);
                        ClearCurrentConsoleLine();
                    }
                    const int align = 10;
                    Console.WriteLine($"Y: {telem.Yaw, align:F4} P: {telem.Pitch,align:F4} R: {telem.Roll,align:F4}");
                    Console.WriteLine($"x: {telem.X,align:F4} y: {telem.Y,align:F4} z: {telem.Z,align:F4}");

                    Thread.Sleep(16);


                }
            }).Start();

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
            cts.Cancel();
            Console.WriteLine("Quitting ..");
            Console.ReadKey();
        }

        private static T bytesToStruct<T>(byte[] bytes) where T : struct
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T theStructure = Marshal.PtrToStructure<T>(handle.AddrOfPinnedObject());
            handle.Free();

            return theStructure;
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
    public struct DistanceTelemetryData
    {
        public int PacketId;
        public float X;
        public float Y;
        public float Z;
        public float Yaw;
        public float Pitch;
        public float Roll;        
    }
}
