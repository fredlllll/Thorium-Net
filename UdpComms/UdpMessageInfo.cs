using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Thorium.Net.UdpComms
{
    public class UdpMessageInfo
    {
        public readonly UdpMessageData data;
        public readonly UdpMsgSuccessCallback successCallback;
        public readonly UdpMsgFailureCallback failureCallback;
        public readonly DateTime endOfRetries;

        public UdpMessageInfo(UdpMessageData data, UdpMsgSuccessCallback successCallback, UdpMsgFailureCallback failureCallback, TimeSpan retryTime)
        {
            this.data = data;
            this.successCallback = successCallback;
            this.failureCallback = failureCallback;
            endOfRetries = DateTime.UtcNow + retryTime;
        }
    }
}
