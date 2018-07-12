using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouteDirector
{
	class HeartBeat : MessageBase
	{
		public byte[] messageBuf;
		static public Int16 messageId = (Int16)MessageBase.MessageType.HeartBeat;
		public Int16 period;

		static public int len = 4;

		/// <summary>
		/// 解析消息数组至单个消息对象
		/// </summary>
		/// <param name="buf">消息数组</param>
		/// <param name="offset">数组偏移量</param>
		public HeartBeat(byte[] buf, int offset) : base(messageId)
		{
			offset += 2;
			offset += DataConversion.ByteToNum(buf, offset, ref period, false);
			Pack();
		}

		/// <summary>
		/// 根据参数创建消息对象
		/// </summary>
		/// <param name="mPeriod">心跳间隔s</param>
		public HeartBeat(Int16 mPeriod) : base(messageId)
		{
			period = mPeriod;
			Pack();
		}

		private void Pack()
		{
			messageBuf = DataConversion.NumToByte(msgId, false);
			messageBuf = DataConversion.NumToByte(period, messageBuf, false);
			base.msgBuf = messageBuf;
		}

		/// <summary>
		/// 打印消息的参数信息
		/// </summary>
		/// <param name="str">StringBuilder引用</param>
		/// <returns>StringBuilder引用</returns>
		public override StringBuilder GetInfo(StringBuilder str)
		{
			str = base.GetInfo(str);
			str.AppendLine("period = " + period.ToString());
			str.AppendLine();
			return str;
		}
	}
}
