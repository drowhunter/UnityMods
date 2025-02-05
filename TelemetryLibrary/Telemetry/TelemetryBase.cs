using System;
using System.Runtime.InteropServices;
#if !NET2_0
using System.Threading;
using System.Threading.Tasks;
#endif
namespace TelemetryLib.Telemetry
{
    internal interface ITelemetry<TData, TConfig> 
        where TData : struct 
        where TConfig : class, new()
    {
        event TelemetryBase<TData, TConfig>.LogEventHandler OnLog;

        //void Configure(TConfig config);       


        int Send(TData data);

        TData Receive();

#if !NET2_0
        Task<TData> ReceiveAsync(CancellationToken cancellationToken = default);
#endif
    }

    internal abstract class TelemetryBase<TData, TConfig> : ITelemetry<TData, TConfig>, IDisposable
        where TData : struct
        where TConfig : class, new()
    {
        public TConfig Config { get; private set; }

        public delegate void LogEventHandler(object sender, string message);
        public event LogEventHandler OnLog;

        protected abstract void Configure(TConfig config);
        public abstract int Send(TData message);
        public abstract TData Receive();

        protected TelemetryBase(TConfig config)
        {
            Config = config ?? new TConfig();
            Configure(Config);
        }

        protected void Log(string message)
        {
            OnLog?.Invoke(this, $"[{this.GetType().Name}] " + message);
        }

        virtual protected byte[] ToBytes<T>(T data) where T : struct
        {
            int size = Marshal.SizeOf(data);
            byte[] arr = new byte[size];
            using (SafeBuffer buffer = new SafeBuffer(size))
            {
                Marshal.StructureToPtr(data, buffer.DangerousGetHandle(), true);
                Marshal.Copy(buffer.DangerousGetHandle(), arr, 0, size);
            }
            return arr;
        }

        //virtual protected T FromBytes<T>(byte[] data) where T : struct
        //{
        //    T result = default(T);
        //    using (SafeBuffer buffer = new SafeBuffer(data.Length))
        //    {
        //        Marshal.Copy(data, 0, buffer.DangerousGetHandle(), data.Length);
        //        result = (T)Marshal.PtrToStructure(buffer.DangerousGetHandle(), typeof(T));
        //    }
        //    return result;
        //}

        virtual protected T FromBytes<T>(byte[] data) where T : struct
        {
            T result = default(T);

            GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            result = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();
            return result;

        }

        public abstract void Dispose();

#if !NET2_0
        public abstract Task<TData> ReceiveAsync(CancellationToken cancellationToken = default);
#endif
    }

    internal class SafeBuffer : SafeHandle
    {
        public SafeBuffer(int size) : base(IntPtr.Zero, true)
        {
            SetHandle(Marshal.AllocHGlobal(size));
        }

        public override bool IsInvalid => handle == IntPtr.Zero;

        protected override bool ReleaseHandle()
        {
            Marshal.FreeHGlobal(handle);
            return true;
        }
    }
}
