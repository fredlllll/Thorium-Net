using System;
using System.Collections.Generic;
using System.Text;
using Thorium.Utils;
using System.Linq;

namespace Thorium.Net.UdpComms
{
    public class UdpMessageAnswerData : UdpMessageData
    {
        public readonly uint answerToId;

        public UdpMessageAnswerData(uint id, uint answerToId, byte[] bytes) : base(id, bytes)
        {
            this.answerToId = answerToId;
        }

        public override byte[] GetBytes()
        {
            byte messageType = (byte)UdpMessageDataType.Answer;
            byte[] idBytes = BitConverterHelper.GetBytesBigEndian(id);
            byte[] answerToIdBytes = BitConverterHelper.GetBytesBigEndian(answerToId);

            byte[] newBytes = idBytes.Prepend(messageType).Concat(answerToIdBytes).Concat(bytes).ToArray();

            return newBytes;
        }

        public static new UdpMessageAnswerData FromBytes(byte[] msgBytes)
        {
            byte messageType = msgBytes[0];
            VerifyMessageDataType(UdpMessageDataType.Answer, messageType);
            uint id = BitConverter.ToUInt32(msgBytes, 1);
            uint answerId = BitConverter.ToUInt32(msgBytes, 5);
            byte[] bytes = msgBytes.AsMemory(9).ToArray();

            return new UdpMessageAnswerData(id, answerId, bytes);
        }
    }
}
