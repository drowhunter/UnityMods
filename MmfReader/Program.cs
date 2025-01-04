using System.Linq.Expressions;
using System.Net;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.Json;

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

                var cs = new SelectorDictionary<DistanceTelemetryData>(10);

               
                Console.WriteLine("Start Read Thread");
                var ms = new MemoryStream();
                
                while (!cts.Token.IsCancellationRequested)
                {
                    var telem = udp.Receive();
                    for (var i = 1; i < 15; i++)
                    {
                        Console.SetCursorPosition(0, i);
                        ClearCurrentConsoleLine();
                    }

                    Console.SetCursorPosition(0, 2);
                    const int align = 10;

                    //cs.Print(telem);

                    cs.LogLine(telem, _ => _.Finished);
                    cs.LogLine(telem, _ => _.Yaw, _ => _.Pitch, _ => _.Roll);
                    cs.LogLine(telem, _ => _.KPH, _ => _.Sway, _ => _.Mass);
                    Console.WriteLine();
                    cs.LogLine(telem, nameof(DistanceTelemetryData.Velocity), _ => _.Velocity.X, _ => _.Velocity.Y, _ => _.Velocity.Z);
                    cs.LogLine(telem, nameof(DistanceTelemetryData.Accel), _ => _.Accel.X, _ => _.Accel.Y, _ => _.Accel.Z);
                    cs.LogLine(telem, _ => _.WingsOpen, _ => _.Boost, _ => _.Grip, _ => _.AllWheelsOnGround);
                    Console.WriteLine();
                    Console.WriteLine("Tires\n");

                    cs.LogLine(telem, nameof(DistanceTelemetryData.TireFL), _ => _.TireFL.Contact, _ => _.TireFL.Position);
                    cs.LogLine(telem, nameof(DistanceTelemetryData.TireFR), _ => _.TireFR.Contact, _ => _.TireFR.Position);
                    cs.LogLine(telem, nameof(DistanceTelemetryData.TireBL), _ => _.TireBL.Contact, _ => _.TireBL.Position);
                    cs.LogLine(telem, nameof(DistanceTelemetryData.TireBR), _ => _.TireBR.Contact, _ => _.TireBR.Position);
                    


                }
            }).Start();

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
            cts.Cancel();
            Console.WriteLine("Quitting ..");
            Console.ReadKey();
        }


        class SelectorDictionary<T>
        {
            private readonly int align;

            private static Dictionary<string, Func<T, object>> _selectors = new();

            public SelectorDictionary(int align = 7)
            {
                this.align = align;
            }


            public void LogLine(T telem, params Expression<Func<T, object>>[] selectors)
            {
                LogLine(telem, null, selectors);
            }

            public void LogLine(T telem,string label, params Expression<Func<T, object>>[] selectors)
            {
                string line = "";

                foreach (var selector in selectors)
                {

                    var member = selector.Body as MemberExpression ?? ((UnaryExpression)selector.Body).Operand as MemberExpression;
                    var memberName = member.Member.Name;

                    
                    Func<T, object> cs;

                    if (!_selectors.ContainsKey(memberName))
                    {
                        cs = selector.Compile();
                        _selectors[memberName] = cs;
                    }
                    else
                    {
                        cs = _selectors[memberName];
                    }

                    var value = cs(telem);
                    //if((label?.Length ?? 0) > 0)
                        //line += label + '\n' + new string('_', label?.Length ?? 0) + "\n\n";


                    line += string.Format("{2}{0}:\t{1," + align + ":F4}\t", memberName, value, label != null ? label + ".": "");

                }

                Console.WriteLine(line);
            }
        }


        public static void ClearCurrentConsoleLine()
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct DistanceTelemetryData
    {
        public int PacketId;
        public float KPH;
        public float Yaw;
        public float Pitch;
        public float Roll;
        public float Sway;
        public Vector3 Velocity;
        public Vector3 Accel;
        public bool Boost;
        //public bool OnTrack;
        public bool WingsOpen;
        public float Mass;
        public bool Finished;
        public bool AllWheelsOnGround;
        public bool isActiveAndEnabled;
        public bool Grip;
        public Tire TireFL;
        public Tire TireFR;
        public Tire TireBL;
        public Tire TireBR;
    }

    internal struct Tire
    {
       public bool Contact;       
       public float Position;

    }
}
