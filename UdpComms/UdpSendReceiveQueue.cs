using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Thorium.Threading;
using Thorium.Utils;

namespace Thorium.Net.UdpComms
{
    public delegate void UdpMsgSuccessCallback(uint id, byte[] answerBytes);
    public delegate void UdpMsgFailureCallback(uint id);

    public class UdpSendReceiveQueue : RestartableThreadClass
    {
        UdpSendQueue sendQueue;
        UdpReceiveQueue receiveQueue;

        RollingOverUInt idProvider = new RollingOverUInt();

        ConcurrentDictionary<uint, UdpMessageInfo> messages = new ConcurrentDictionary<uint, UdpMessageInfo>();

        public UdpSendReceiveQueue(string remoteHost, int commsPort) : base(false)
        {
            sendQueue = new UdpSendQueue(remoteHost, commsPort);
            sendQueue.FailedToSend += SendQueue_FailedToSend;
            receiveQueue = new UdpReceiveQueue(commsPort);
            receiveQueue.UdpMessageDataReceived += ReceiveQueue_UdpMessageDataReceived;
        }

        private void SendQueue_FailedToSend(uint id)
        {
            messages.TryRemove(id, out UdpMessageInfo msg);
            msg.failureCallback(id);
        }

        private void ReceiveQueue_UdpMessageDataReceived(UdpMessageData data)
        {
            sendQueue.Ack(data);
            messages.TryRemove(data.id, out UdpMessageInfo _);
        }

        public uint SendMessage(byte[] bytes, UdpMsgSuccessCallback successCallback, UdpMsgFailureCallback failureCallback, TimeSpan retryTime)
        {
            uint id = idProvider.Next;
            /*var msg = new UdpMessageInfo(id, bytes, successCallback, failureCallback, retryTime);
            messages[id] = msg;
            sendQueue.Send(msg.data, msg.endOfRetries);*/
            return id;
        }

        public void SendMessage(uint id, byte[] bytes, UdpMsgSuccessCallback successCallback, UdpMsgFailureCallback failureCallback, TimeSpan retryTime)
        {
            /*var msg = new UdpMessageInfo(id, bytes, successCallback, failureCallback, retryTime);
            messages[id] = msg;
            sendQueue.Send(msg.data, msg.endOfRetries);*/
        }

        public override void Start()
        {
            running = true;
            base.Start();
        }

        public override void Stop(int joinTimeoutms = -1)
        {
            running = false;
            base.Stop(joinTimeoutms);
        }

        bool running = false;
        protected override void Run()
        {
            try
            {
                while(running)
                {
                    //messages.
                }
            }
            catch(ThreadInterruptedException)
            {
                //just end
            }
        }


    }
}
