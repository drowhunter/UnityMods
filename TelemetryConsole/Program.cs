﻿using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Net;
using System.Numerics;
using System.Reflection;
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

            //var inputs = GetInputs<DistanceTelemetryData>(default).ToImmutableArray();


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


                    //Console.SetCursorPosition(0, 0);
                    //foreach (var (i, (key, value)) in GetInputs(telem).WithIndex())
                    //{
                    //    Console.WriteLine($"{i} - {key}: {value}");
                    //}

                    //for (var i = 1; i < 20; i++)
                    //{
                    //    Console.SetCursorPosition(0, i);
                    //    ClearCurrentConsoleLine();
                    //}

                    Console.SetCursorPosition(0, 2);
                    const int align = 10;

                    //cs.Print(telem);

                    
                    cs.LogLine(telem, _ => _.Yaw, _ => _.Pitch, _ => _.Roll);
                    cs.LogLine(telem, _ => _.KPH, _ => _.Sway, _ => _.Mass);
                    Console.WriteLine();
                    cs.LogLine(telem, nameof(DistanceTelemetryData.Velocity), _ => _.Velocity.X, _ => _.Velocity.Y, _ => _.Velocity.Z);
                    cs.LogLine(telem, nameof(DistanceTelemetryData.Accel), _ => _.Accel.X, _ => _.Accel.Y, _ => _.Accel.Z);
                    cs.LogLine(telem, nameof(DistanceTelemetryData.Inputs), _ => _.Inputs.Gas, _ => _.Inputs.Brake                        
                    );
                    cs.LogLine(telem, nameof(DistanceTelemetryData.Inputs), _ => _.Inputs.Boost, _ => _.Inputs.Grip, _ => _.Inputs.Wings);
                    cs.LogLine(telem, _ => _.Finished, _ => _.isActiveAndEnabled);
                    cs.LogLine(telem, _ => _.AllWheelsOnGround, _ => _.Grav, _ => _.AngularDrag);

                    Console.WriteLine();
                    Console.WriteLine("Tires\n");

                    cs.LogLine(telem, nameof(DistanceTelemetryData.TireFL), _ => _.TireFL.Contact, _ => _.TireFL.Position, _ => _.TireFL.Suspension );
                    cs.LogLine(telem, nameof(DistanceTelemetryData.TireFR), _ => _.TireFR.Contact, _ => _.TireFR.Position, _ => _.TireFL.Suspension );
                    cs.LogLine(telem, nameof(DistanceTelemetryData.TireBL), _ => _.TireBL.Contact, _ => _.TireBL.Position, _ => _.TireFL.Suspension );
                    cs.LogLine(telem, nameof(DistanceTelemetryData.TireBR), _ => _.TireBR.Contact, _ => _.TireBR.Position, _ => _.TireFL.Suspension );
                    


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

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct DistanceTelemetryData
    {
        public int PacketId;
        public float KPH;
        public float Mass;
        public float Yaw;
        public float Pitch;
        public float Roll;
        public float Sway;
        public Vector3 Velocity;
        public Vector3 Accel;
        public Inputs Inputs;
        public bool Finished;
        public bool AllWheelsOnGround;
        public bool isActiveAndEnabled;
        public bool Grav;
        public float AngularDrag;
        public Tire TireFL;
        public Tire TireFR;
        public Tire TireBL;
        public Tire TireBR;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct Tire
    {
        public bool Contact;
        public float Position;
        public float Suspension;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct Inputs
    {
        public float Gas;
        public float Brake;
        public float Steer;
        public bool Boost;
        public bool Grip;
        public bool Wings;
    }
}
