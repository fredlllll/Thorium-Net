using System;
using System.Collections.Generic;
using System.Text;

namespace Thorium.Net.UdpComms
{
    public enum UdpMessageDataType : byte
    {
        Request = 1,
        Answer = 2
    }

    public abstract class UdpMessageData
    {
        public readonly uint id;
        public readonly byte[] bytes;

        public UdpMessageData(uint id, byte[] bytes)
        {
            this.id = id;
            this.bytes = bytes;
        }

        protected static void VerifyMessageDataType(UdpMessageDataType expected, byte actual)
        {
            VerifyMessageDataType(expected, (UdpMessageDataType)actual);
        }

        protected static void VerifyMessageDataType(UdpMessageDataType expected, UdpMessageDataType actual)
        {
            if(expected != actual)
            {
                throw new InvalidOperationException("this is not a " + expected + " message");
            }
        }

        public abstract byte[] GetBytes();

        public static UdpMessageData FromBytes(byte[] msgBytes)
        {
            byte messageType = msgBytes[0];
            UdpMessageData retVal = null;
            switch(messageType)
            {
                case (byte)UdpMessageDataType.Request:
                    retVal = UdpMessageRequestData.FromBytes(msgBytes);
                    break;
                case (byte)UdpMessageDataType.Answer:
                    retVal = UdpMessageAnswerData.FromBytes(msgBytes);
                    break;
                default:
                    throw new InvalidOperationException("Unknown message type: " + messageType);
            }
            return retVal;
        }
    }
}
