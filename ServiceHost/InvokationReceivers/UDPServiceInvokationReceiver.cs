using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using Newtonsoft.Json.Linq;
using NLog;
using Thorium.Config;
using Thorium.Net.ServiceHost.Invokers;
using Thorium.Threading;

namespace Thorium.Net.ServiceHost.InvokationReceivers
{
    [CompatibleInvoker(typeof(UDPServiceInvoker))]
    public class UDPServiceInvokationReceiver : InvokationReceiver
    {
        UdpClient udpClient;
        bool running = false;

        public UDPServiceInvokationReceiver(Config.Config config) : base(config)
        {
            dynamic c = config;
            udpClient = new UdpClient(c.Port);
        }

        public UDPServiceInvokationReceiver(string configName) : this(ConfigFile.GetConfig(configName)) { }

        public override void Start()
        {
            udpClient.BeginReceive(EndReceive, null);
            running = true;
        }

        public override void Stop()
        {
            running = false;
        }

        void EndReceive(IAsyncResult result)
        {
            if(running)
            {
                /*result.AsyncState
                UdpClient u = (UdpClient)((UdpState)(ar.AsyncState)).u;
                IPEndPoint e = (IPEndPoint)((UdpState)(ar.AsyncState)).e;

                Byte[] receiveBytes = u.EndReceive(ar, ref e);

                udpClient.BeginReceive(EndReceive, null);*/
            }
        }
    }
}
