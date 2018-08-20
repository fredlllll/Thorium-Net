using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Thorium.Threading;

namespace Thorium.Net.UdpComms
{
    public class UdpSendQueue : RestartableThreadClass
    {
        public event Action<uint> FailedToSend;

        readonly UdpClient sender;

        BlockingCollection<UdpMessageData> messages;
        readonly ConcurrentDictionary<uint, DateTime> endOfRetries = new ConcurrentDictionary<uint, DateTime>();
        readonly ConcurrentDictionary<uint, UdpMessageData> ackedMessages = new ConcurrentDictionary<uint, UdpMessageData>();

        public UdpSendQueue(string host, int port) : base(false)
        {
            sender = new UdpClient(host, port);
        }

        public void Send(UdpMessageData msg, DateTime endOfRetries)
        {
            messages.Add(msg);
        }

        /// <summary>
        /// acknowledge that a message has been answered, so it will not be rescheduled
        /// </summary>
        /// <param name="msg"></param>
        public void Ack(UdpMessageData msg)
        {
            ackedMessages[msg.id] = msg;
        }

        void ScheduleForResend(UdpMessageData msg)
        {
            Task t = new Task(async () =>
            {
                await Task.Delay(5000); //wait 5 seconds
                if(!ackedMessages.ContainsKey(msg.id))
                {
                    DateTime endOfRetries = this.endOfRetries[msg.id];
                    if(DateTime.UtcNow <= endOfRetries)
                    {
                        messages.Add(msg); //retry
                    }
                    else
                    {
                        FailedToSend?.Invoke(msg.id);//signal failed transmission
                    }
                }
                else
                {
                    //remove if it was acked
                    ackedMessages.TryRemove(msg.id, out msg);
                    endOfRetries.TryRemove(msg.id, out DateTime dt);
                }
            });
            t.Start();
        }

        public override void Start()
        {
            messages = new BlockingCollection<UdpMessageData>();
            ackedMessages.Clear();
            base.Start();
        }

        public override void Stop(int joinTimeoutms = -1)
        {
            base.Stop(joinTimeoutms);
            messages = null;
            ackedMessages.Clear();
        }

        protected override void Run()
        {
            while(true)
            {
                try
                {
                    UdpMessageData msg = messages.Take();
                    //using async here because im unsure if sync send can throw threadinterrupted exception
                    byte[] bytes = msg.GetBytes();
                    sender.BeginSend(bytes, bytes.Length, (ar) => { }, null);
                    ScheduleForResend(msg);
                }
                catch(ThreadInterruptedException)
                {
                    break; //leave
                }
            }
        }
    }
}
