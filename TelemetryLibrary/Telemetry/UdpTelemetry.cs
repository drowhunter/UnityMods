using System;
using System.Net;
using System.Net.Sockets;
#if !NET2_0
using System.Threading;
using System.Threading.Tasks;
#endif

namespace TelemetryLib.Telemetry
{
    internal class UdpTelemetryConfig
    {
        public IPEndPoint SendAddress { get; set; }

        public IPEndPoint ReceiveAddress { get; set; }

        public int ReceiveTimeout { get; set; } = 0;
    }

    internal class UdpTelemetry<TData> : TelemetryBase<TData, UdpTelemetryConfig>
        where TData : struct
    {
        private static UdpClient udpClient;

        public UdpTelemetry(UdpTelemetryConfig config) : base(config)
        {
        }

        protected override void Configure(UdpTelemetryConfig config)
        {
            if (config.ReceiveAddress != null)
            {
                Log($"Create UdpClient: Receiving @ {config.ReceiveAddress.Address}: {config.ReceiveAddress.Port} with timeout of {Config.ReceiveTimeout} ms");
                udpClient = new UdpClient(config.ReceiveAddress);                
            }
            else
            {
                Log($"Create UdpClient");
                udpClient = new UdpClient();
            }

            if (config.SendAddress != null)
            {
                Log($"Create Send Adress {config.SendAddress.Address}: {config.SendAddress.Port} with timeout of {Config.ReceiveTimeout} ms");
                udpClient.Connect(config.SendAddress);
            }

            udpClient.Client.ReceiveTimeout = Config.ReceiveTimeout;
        }

        public override TData Receive()
        {
            IPEndPoint remoteEp = null;
            var data = udpClient.Receive(ref remoteEp);

            return FromBytes<TData>(data);

        }

        public override int Send(TData data)
        {
            var bytes = ToBytes(data);
            return udpClient.Send(bytes, bytes.Length);
        }

        public override void Dispose()
        {
            udpClient.Close();
        }

#if !NET2_0
        public override async Task<TData> ReceiveAsync(CancellationToken cancellationToken = default)
        { 
            TData data;

#if NET6_0_OR_GREATER
            var result = await udpClient.ReceiveAsync(cancellationToken);
            data = FromBytes<TData>(result.Buffer);
#else
            var result = await udpClient.ReceiveAsync();
            data = FromBytes<TData>(result.Buffer);
#endif

            return data;
        }
#endif
    }
}
