using System;
using System.Collections.Generic;
using System.Text;

namespace Thorium.Net.UdpComms
{
    public class UdpMessageRequestData : UdpMessageData
    {
        public UdpMessageRequestData(uint id, byte[] bytes) : base(id, bytes)
        { }

        public override byte[] GetBytes()
        {
            byte messageType = (byte)UdpMessageDataType.Request;
            byte[] idBytes = BitConverter.GetBytes(id);
            if(BitConverter.IsLittleEndian) //transfer in big endian
            {
                Array.Reverse(idBytes);
            }

            byte[] newBytes = new byte[1 + idBytes.Length + bytes.Length];
            newBytes[0] = messageType;
            for(int i = 0; i < idBytes.Length; i++)
            {
                newBytes[i + 1] = idBytes[i];
            }
            int offset = idBytes.Length;
            for(int i = 0; i < bytes.Length; i++)
            {
                newBytes[i + offset] = bytes[i];
            }
            return newBytes;
        }

        public static new UdpMessageRequestData FromBytes(byte[] msgBytes)
        {
            byte messageType = msgBytes[0];
            VerifyMessageDataType(UdpMessageDataType.Request, messageType);
            uint id = BitConverter.ToUInt32(msgBytes, 1);
            byte[] bytes = msgBytes.AsMemory(5).ToArray();

            return new UdpMessageRequestData(id, bytes);
        }
    }
}
