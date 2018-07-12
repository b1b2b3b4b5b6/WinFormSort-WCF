
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouteDirector
{
	public class MessageBase
	{
		public enum MessageType : Int16
		{
			NoType = 0,
			DivertReq = 1,
			DivertCmd = 2,
			DivertRes = 3,
			HeartBeat = 9,
			NodeAva = 11,
			CommsErr = 12,
		}
		public byte[] msgBuf;
		public Int16 msgId;

		public MessageBase()
		{
			msgId = (Int16)MessageType.NoType;
		}

		public MessageBase(Int16 tMsgId)
		{
			msgId = tMsgId;
		}

		/// <summary>
		/// 解析消息数组至单个消息对象
		/// </summary>
		/// <param name="buf">消息数组</param>
		/// <param name="offset">偏移量引用</param>
		/// <returns>消息对象</returns>
		static public MessageBase MessageCreate(byte[] buf, ref int offset)
		{
			Int16 tMsgId = 0;
			MessageBase messageBase;
			DataConversion.ByteToNum(buf, offset, ref tMsgId, false);
			switch (tMsgId)
			{
				case (Int16)MessageType.DivertReq:
					messageBase = new DivertReq(buf, offset);
					offset += DivertReq.len;
					break;

				case (Int16)MessageType.DivertRes:
					messageBase = new DivertRes(buf, offset);
					offset += DivertRes.len;
					break;

				case (Int16)MessageType.HeartBeat:
					messageBase = new HeartBeat(buf, offset);
					offset += HeartBeat.len;
					break;

				case (Int16)MessageType.NodeAva:
					messageBase = new NodeAva(buf, offset);
					offset += NodeAva.len;
					break;

				case (Int16)MessageType.CommsErr:
					messageBase = new CommsErr(buf, offset);
					offset += CommsErr.len;
					break;
				default:
					throw new NotImplementedException();
					
			}
			return messageBase;
		}

		/// <summary>
		/// 打印消息的参数信息
		/// </summary>
		/// <param name="str">StringBuilder引用</param>
		/// <returns>StringBuilder引用</returns>
		public virtual StringBuilder GetInfo(StringBuilder str)
		{
			Func<Int16, String> GetName = ((value) => {
				return Enum.GetName(typeof(MessageType), value);
			});
			str.AppendLine("*************Message*************");
			str.AppendLine("Message type : " + GetName(msgId));
			str.AppendLine("Message length : " + msgBuf.Length.ToString());
			return str;
		}
	}
}
