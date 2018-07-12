using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RouteDirector
{
	public class Packet
	{
		public enum Identification : Int16
		{
			PLC1 = 21,
			PLC2 = 22,
			PLC9 = 29,
			RouteDirector = 30,
			InductManager = 31,
		}

		public enum TransportError : Int16
		{
			NoErr = 0,
			CommLinkErr = 1,
			CycleNumERR = 2,
			HanderFormatErr = 3,
			AckBufOverflowErr = 4,
			AckSeqErr = 5,
		}
		const int packetMaxLength = 240;

		public Int16 cycleNum;
		public Int16 senderId;
		public Int16 receiverId;
		public Int16 ack;
		public Int16 transportError;

		public  List<MessageBase> messageList = new List<MessageBase>();
		private byte[] packetBuf;
		/// <summary>
		/// 获取报文数组
		/// </summary>
		/// <returns>报文数组</returns>
		public byte[] GetBuf()
		{
			pack();
			return packetBuf;
		}

		/// <summary>
		/// 解析报文数组至报文对象
		/// </summary>
		/// <param name="buf">报文数组</param>
		public Packet(byte[] buf)
		{
			int offset = 0;
			if (buf.Length > packetMaxLength)
				throw new NotImplementedException();
			packetBuf = (byte[])buf.Clone();
			offset += DataConversion.ByteToNum(buf, offset, ref cycleNum, false);
			offset += DataConversion.ByteToNum(buf, offset, ref senderId, false);
			offset += DataConversion.ByteToNum(buf, offset, ref receiverId, false);
			offset += DataConversion.ByteToNum(buf, offset, ref ack, false);
			offset += DataConversion.ByteToNum(buf, offset, ref transportError, false);
			messageList = UnpackMessage(buf, offset);
		}


		public Packet()
		{
			packetInit(0);
		}

		/// <summary>
		/// 为报文添加一条消息
		/// </summary>
		/// <param name="messageBase">消息</param>
		public void AddMsg(MessageBase messageBase)
		{
			messageList.Add(messageBase);
		}
		/// <summary>
		/// 为报文对象添加序列号
		/// </summary>
		/// <param name="tCycleNum">报文序列号</param>
		///<param name="tCycleNum">应答序列号</param>
		public void AddCycleNum(Int16 tCycleNum, Int16 tAck)
		{
			cycleNum = tCycleNum;
			ack = tAck;
		}
		/// <summary>
		/// 为报文添加多条消息
		/// </summary>
		/// <param name="messageBase">消息列表</param>
		public void AddMsgList(List<MessageBase> tMessageList)
		{
			messageList = tMessageList;
		}

		private void packetInit(Int16 tAck)
		{
			AddCycleNum(0, 0);
			senderId = (Int16)Identification.RouteDirector;
			receiverId = (Int16)Identification.PLC1;
			transportError = 0;
		}

		/// <summary>
		/// 打包报文对象至报文数组
		/// </summary>
		private void pack()
		{
			packetBuf = DataConversion.NumToByte(cycleNum, false);
			packetBuf = DataConversion.NumToByte(senderId, packetBuf, false);
			packetBuf = DataConversion.NumToByte(receiverId, packetBuf, false);
			packetBuf = DataConversion.NumToByte(ack, packetBuf, false);
			packetBuf = DataConversion.NumToByte(transportError, packetBuf, false);
			packetBuf = packetBuf.Concat(PackMessage(messageList)).ToArray();
			int len = 240 - packetBuf.Length;
			byte[] padding = Enumerable.Repeat((byte)0xff, len).ToArray();
			packetBuf = packetBuf.Concat(padding).ToArray();
		}

		/// <summary>
		/// 解析消息数组至消息对象列表
		/// </summary>
		/// <param name="buf">消息数组</param>
		/// <param name="offset">数组偏移量</param>
		/// <returns>消息对象列表</returns>
		private List<MessageBase> UnpackMessage(byte[] buf, int offset)
		{
			int len = buf.Length;
			List<MessageBase> tMsgList = new List<MessageBase>();
 			while (true)
			{
				if (DataConversion.ByteToNum<Int16>(buf, offset, false) == -1)
				{
					offset += 2;
					if (DataConversion.ByteToNum<Int16>(buf, offset, false) == -1)
					{
						offset += 2;
						packetBuf = (byte[])buf.Take(offset).ToArray();
						break;
					}
					else
					{
						MessageBase message = MessageBase.MessageCreate(buf, ref offset);
						tMsgList.Add(message);
					}
				}
				if(offset >= (packetMaxLength - 10))
					throw new NotImplementedException();
			}
			if (tMsgList.Count() <= 0)
				throw new NotImplementedException();
			return tMsgList;
		}

		/// <summary>
		/// 打包消息对象列表至消息数组
		/// </summary>
		/// <param name="tMessageList">消息对象数组</param>
		/// <returns>消息数组</returns>
		private byte[] PackMessage(List<MessageBase> tMessageList)
		{
			byte[] buf = null;
			int len = tMessageList.Count;
			
			if (len <= 0)
				throw new NotImplementedException();
			foreach (MessageBase messageBase in tMessageList)
			{
				buf = DataConversion.NumToByte((Int16)(-1), buf, false);
				buf = buf.Concat(messageBase.msgBuf).ToArray();
			}
			return buf;
		}

		/// <summary>
		/// 打印报文对象的所有信息
		/// </summary>
		/// <param name="str">StringBuilder引用</param>
		/// <returns>StringBuilder引用</returns>
		public StringBuilder GetInfo(StringBuilder str)
		{
			Func<Int16, String> GetName = ((value) => {
				return Enum.GetName(typeof(Identification), value);
			});

			str.AppendLine("-----------------packet-----------------");
			str.AppendLine("packet length : " + packetBuf.Length.ToString());
			str.AppendLine("message amount : " + messageList.Count());
			str.AppendLine("cycleNum = " + cycleNum.ToString());
			str.AppendLine("senderId = " + GetName(senderId));
			str.AppendLine("receiverId = " + GetName(receiverId));
			str.AppendLine("ack = " + ack.ToString());
			str.AppendLine("transportError = " + Enum.GetName(typeof(TransportError), transportError));
			str.AppendLine();
			foreach (MessageBase messageBase in messageList)
			{
				messageBase.GetInfo(str);
			}
			str.AppendLine("---------------packet end---------------");
			str.AppendLine();
			return str;
		}
	}
}
