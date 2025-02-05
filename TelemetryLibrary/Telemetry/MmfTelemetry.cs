#if !NET2_0
using System.Threading;
using System.Threading.Tasks;
using System.IO.MemoryMappedFiles;
#endif

using System.Runtime.InteropServices;

namespace TelemetryLib.Telemetry
{
    internal class MmfTelemetryConfig
    {
        public string Name { get; set; } = "MmfTelemetry";        
    }

    internal class MmfTelemetry<TData> : TelemetryBase<TData, MmfTelemetryConfig>
        where TData : struct
    {
#if !NET2_0
        private MemoryMappedFile _mmf;
        private MemoryMappedViewAccessor _accessor;
        private int _dataSize = Marshal.SizeOf<TData>();

        //static Mutex mutex;
#endif

        public MmfTelemetry(MmfTelemetryConfig config) : base(config)
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>")]
        protected override void Configure(MmfTelemetryConfig config)
        {
#if !NET2_0
            _mmf = MemoryMappedFile.CreateOrOpen(config.Name, Marshal.SizeOf<TData>());

            _accessor = _mmf.CreateViewAccessor();
#endif
        }


        public override TData Receive()
        {
#if !NET2_0
            _accessor.Read(0, out TData data);
            return data;

#else
            return default;
#endif
        }



        public override int Send(TData data)
        {
#if !NET2_0

            _accessor.Write(0, ref data);
            return _dataSize;

#else
            return 0;
#endif


        }

        public override void Dispose()
        {
#if !NET2_0
            _mmf?.Dispose();

            _accessor?.Dispose();
#endif
        }

#if !NET2_0
        public override Task<TData> ReceiveAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Receive());
        }
#endif
    }
}
