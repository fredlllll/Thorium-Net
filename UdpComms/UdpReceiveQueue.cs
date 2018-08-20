using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Thorium.Net.UdpComms
{
    public delegate void UdpMessageDataReceived(UdpMessageData data);

    public class UdpReceiveQueue
    {
        public event UdpMessageDataReceived UdpMessageDataReceived;

        readonly UdpClient receiver;
        bool running = false;

        public UdpReceiveQueue(int port)
        {
            receiver = new UdpClient(port);
        }

        void Start()
        {
            running = true;
            receiver.BeginReceive(HandleReceive, null);
        }

        void Stop()
        {
            running = false;
        }

        void HandleReceive(IAsyncResult result)
        {
            if(running)
            {
                IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] bytes = receiver.EndReceive(result, ref endPoint);
                var data = UdpMessageData.FromBytes(bytes);
                UdpMessageDataReceived?.Invoke(data);

                receiver.BeginReceive(HandleReceive, null);
            }
        }
    }
}
