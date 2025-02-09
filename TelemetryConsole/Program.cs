using System.Linq.Expressions;
using System.Net;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;

using TelemetryLib.Telemetry;

namespace MmfReader
{
    internal class Program
    {
        static CancellationTokenSource cts = new CancellationTokenSource();

        static void Main(string[] args)
        {

            //var inputs = GetInputs<DistanceTelemetryData>(default).ToImmutableArray();
            var len = Guid.NewGuid().ToString("B").Length;

            new Thread(static async () =>
            {
                var udp = new UdpTelemetry<DistanceTelemetryData>(new UdpTelemetryConfig
                {
                    ReceiveAddress = new IPEndPoint(IPAddress.Any, 12345)
                });

                var cs = new SelectorDictionary<DistanceTelemetryData>(10);

               
                Console.WriteLine("Start Read Thread");
                var ms = new MemoryStream();
                int oo = 0;
                while (!cts.Token.IsCancellationRequested)
                {
                    var telem = udp.Receive();


                    Console.SetCursorPosition(0, 2);

                    cs.LogLine(telem, _ => _.GamePaused, _ => _.IsRacing);
                    cs.LogLine(telem, _ => _.IsCarIsActive, _ => _.IsCarEnabled, _ => _.IsCarDestroyed);
                    Console.WriteLine();

                    cs.LogLine(telem, nameof(DistanceTelemetryData.Rotation), _ => _.Rotation.X, _ => _.Rotation.Y, _ => _.Rotation.Z);

                    cs.LogLine(telem, nameof(DistanceTelemetryData.Orientation), _ => _.Orientation.w, _ => _.Orientation.x, _ => _.Orientation.y, _ => _.Orientation.z);


                    cs.LogLine(telem, _ => _.KPH, _ => _.cForce, _ => _.IsGrav);
                    Console.WriteLine();
                    cs.LogLine(telem, nameof(DistanceTelemetryData.AngularVelocity), _ => _.AngularVelocity.X, _ => _.AngularVelocity.Y, _ => _.AngularVelocity.Z);

                    cs.LogLine(telem, nameof(DistanceTelemetryData.Velocity), _ => _.Velocity.X, _ => _.Velocity.Y, _ => _.Velocity.Z);
                    cs.LogLine(telem, nameof(DistanceTelemetryData.Accel), _ => _.Accel.X, _ => _.Accel.Y, _ => _.Accel.Z);
                    cs.LogLine(telem, _ => _.Boost, _ => _.Grip, _ => _.WingsOpen);
                    
                    cs.LogLine(telem, _ => _.AllWheelsOnGround);

                    Console.WriteLine();
                    Console.WriteLine("Tires\n");

                    cs.LogLine(telem, _ => _.TireFL, _ => _.TireFR );
                    cs.LogLine(telem, _ => _.TireBL , _ => _.TireBR);
                    
                }
            }).Start();

            Console.WriteLine("Press Enter to exit");
            Console.ReadLine();
            cts.Cancel();
            Console.Clear();
            Console.WriteLine("Quitting ..");
            
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


                    line += string.Format("{2}{0}:\t{1," + align + ":F3}\t", memberName, value, label != null ? label + ".": "");

                }

                Console.WriteLine(line);
            }
        }

        private static IEnumerable<(string key, float value)> GetInputs<T>(T data )
        {
            foreach (var field in (data?.GetType() ?? typeof(T)).GetFields())
            {
                if (field.FieldType.IsPrimitive)
                    yield return (field.Name, GetFloat(field, data));
                else                
                    foreach (var (k, v) in GetInputs(field.GetValue(data)))
                        yield return (field.Name + "." + k, v);                
            }

            float GetFloat(FieldInfo f, object ? data = null)
            {
                var retval = data == null ? 0 : (float)Convert.ChangeType(f.GetValue(data), typeof(float));
                return retval;
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

    internal static class Extensions
    {
        
        internal static IEnumerable<(int index, T item)> WithIndex<T>(this IEnumerable<T> source)
        {
            return source.Select((item, index) => (index, item));
        }
        
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct DistanceTelemetryData
    {
        public bool GamePaused;
        public bool IsRacing;
        public float KPH;

        public Vector3 Rotation;

        public Vector3 AngularVelocity;

        public float cForce;

        public Vector3 Velocity;
        public Vector3 Accel;

        public bool Boost;
        public bool Grip;
        public bool WingsOpen;

        public bool IsCarEnabled;
        public bool IsCarIsActive;
        public bool IsCarDestroyed;
        public bool AllWheelsOnGround;
        public bool IsGrav;

        public float TireFL;
        public float TireFR;
        public float TireBL;
        public float TireBR;

        public Quat Orientation;
    }

    internal struct Quat
    {
        public float w;
        public float x;
        public float y;
        public float z;
    }
}
