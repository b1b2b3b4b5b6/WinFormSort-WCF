using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouteDirector
{
	public class DivertCmd : MessageBase
	{
		public byte[] messageBuf;
		static public Int16 messageId = (Int16)MessageBase.MessageType.DivertCmd;
		public Int16 nodeId = 0;
		public Int16 cartSeq = 0;
		public Int16 priority = 0;
		public Int16 laneId = 0;
		static public int len = 10;

		/// <summary>
		/// 解析消息数组至单个消息对象
		/// </summary>
		/// <param name="buf">消息数组</param>
		/// <param name="offset">数组偏移量</param>
		public DivertCmd(byte[] buf, int offset) : base(messageId)
		{
			offset += 2;
			offset += DataConversion.ByteToNum(buf, offset, ref nodeId, false);
			offset += DataConversion.ByteToNum(buf, offset, ref cartSeq, false);
			offset += DataConversion.ByteToNum(buf, offset, ref priority, false);
			offset += DataConversion.ByteToNum(buf, offset, ref laneId, false);
			Pack();
		}

		/// <summary>
		/// 根据参数创建消息对象
		/// </summary>
		/// <param name="tNodeId">与DivreqReq相同</param>
		/// <param name="tCartSeq">与DivreqReq相同</param>
		/// <param name="tPriority">优先级</param>
		/// <param name="tLaneId">分拣线路</param>
		public DivertCmd(Int16 tNodeId, Int16 tCartSeq, Int16 tPriority, Int16 tLaneId) : base(messageId)
		{
			nodeId = tNodeId;
			cartSeq = tCartSeq;
			priority = tPriority;
			laneId = tLaneId;
			Pack();
		}

		/// <summary>
		/// 根据DivertReq对象创建消息对象
		/// </summary>
		/// <param name="divertReq">DivertReq对象</param>
		/// <param name="tLaneId">分拣线路</param>
		public DivertCmd(DivertReq divertReq, Int16 tLaneId) : base(messageId)
        {
            nodeId = divertReq.nodeId;
            cartSeq = divertReq.cartSeq;
            priority = 0;
            laneId = tLaneId;
            Pack();
        }

        private void Pack()
		{
			messageBuf = DataConversion.NumToByte(msgId, false);
			messageBuf = DataConversion.NumToByte(nodeId, messageBuf, false);
			messageBuf = DataConversion.NumToByte(cartSeq, messageBuf, false);
			messageBuf = DataConversion.NumToByte(priority, messageBuf, false);
			messageBuf = DataConversion.NumToByte(laneId, messageBuf, false);
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
			str.AppendLine("nodeId = " + nodeId.ToString());
			str.AppendLine("cartSeq = " + cartSeq.ToString());
			str.AppendLine("priority = " + priority.ToString());
			str.AppendLine("laneId = " + laneId.ToString());
			str.AppendLine();
			return str;
		}
	}
}
