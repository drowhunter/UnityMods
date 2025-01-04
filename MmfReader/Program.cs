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

                var cs = new SelectorDictionary(10);//<DistanceTelemetryData>();

                var tlm = new DistanceTelemetryData();
                //cs.Add(tlm, _ => _.Finished, _ => _.isActiveAndEnabled);
                //cs.Add(tlm, _ => _.Yaw, _ => _.Pitch, _ => _.Roll);
                //cs.Add(tlm, _ => _.KPH, _ => _.Sway, _ => _.Boost, _ => _.Mass);
                //cs.Add(tlm, _ => _.Velocity.X, _ => _.Velocity.Y, _ => _.Velocity.Z);
                //cs.Add(tlm, _ => _.Accel.X, _ => _.Accel.Y, _ => _.Accel.Z);

                //cs.Add(tlm, _ => _.WingsEnabled, _ => _.WingsOpen);
                //cs.Add(tlm, _ => _.TireFL.IsInContact, _ => _.TireFL.LocalPosition);
                //cs.Add(tlm, _ => _.TireFR.IsInContact, _ => _.TireFR.LocalPosition);
                //cs.Add(tlm, _ => _.TireBL.IsInContact, _ => _.TireBL.LocalPosition);
                //cs.Add(tlm, _ => _.TireBR.IsInContact, _ => _.TireBR.LocalPosition);

                Console.WriteLine("Start Read Thread");
                var ms = new MemoryStream();
                
                while (!cts.Token.IsCancellationRequested)
                {
                    var telem = await udp.ReceiveAsync(cts.Token);

                    //for (var i = 1; i < 30; i++)
                    //{
                    //    Console.SetCursorPosition(0, i);
                    //    ClearCurrentConsoleLine();
                    //}
                    
                    Console.SetCursorPosition(0, 2);
                    const int align = 10;

                    //cs.Print(telem);

                    cs.LogLine(telem, _ => _.Finished, _ => _.isActiveAndEnabled);
                    cs.LogLine(telem, _ => _.Yaw, _ => _.Pitch, _ => _.Roll);
                    cs.LogLine(telem, _ => _.KPH, _ => _.Sway, _ => _.Boost, _ => _.Mass);
                    cs.LogLine(telem.Velocity, _ => _.X, _ => _.Y, _ => _.Z);
                    cs.LogLine(telem.Accel, _ => _.X, _ => _.Y, _ => _.Z);
                    cs.LogLine(telem, _ => _.WingsEnabled, _ => _.WingsOpen);
                    cs.LogLine(telem.TireFL, _ => _.IsInContact, _ => _.LocalPosition);
                    cs.LogLine(telem.TireFR, _ => _.IsInContact, _ => _.LocalPosition);
                    cs.LogLine(telem.TireBL, _ => _.IsInContact, _ => _.LocalPosition);
                    cs.LogLine(telem.TireBR, _ => _.IsInContact, _ => _.LocalPosition);

                    Thread.Sleep(16);


                }
            }).Start();

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
            cts.Cancel();
            Console.WriteLine("Quitting ..");
            Console.ReadKey();
        }


        class SelectorDictionary
        {
            private readonly int align;

            private static Dictionary<string, Delegate> _selectors = new();

            public SelectorDictionary(int align = 10)
            {
                this.align = align;
            }



            public void LogLine<T>(T telem, params Expression<Func<T, object>>[] selectors)
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
                        cs = (Func<T, object>)_selectors[memberName];
                    }

                    var value = cs(telem);

                    line += string.Format("{0}:\t{1," + align + ":F4}\t", memberName, value);

                }

                Console.WriteLine(line);
            }
        }

       

        

        public static void PrettyLog<T>(T telem) //where T : struct
        {
            var fields = typeof(T).GetFields();
            const int align = 10;

            foreach (var field in fields)
            {
                //if T is a number or string or bool, just print it
                if (field.FieldType.IsPrimitive || field.FieldType == typeof(string))
                {
                    Console.Write($"{field.Name}: {field.GetValue(telem),align:F4}" + Environment.NewLine);
                    
                }
                else
                {
                    //if T is a struct, recurse through it
                    PrettyLog(field.GetValue(telem));
                }
               
            }
        }

        private static void PrettyPrint<T>(T telem) where T : struct
        {
            Console.SetCursorPosition(0, 0);
            //Console Write Json indented using System.Text.Json
            Console.WriteLine(JsonSerializer.Serialize(telem, new JsonSerializerOptions { WriteIndented = true }));

            
            //return JsonConvert.SerializeObject(myclass, Newtonsoft.Json.Formatting.Indented);
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
        public bool WingsEnabled;
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
       public bool IsInContact;       
       public float LocalPosition;

    }
}
