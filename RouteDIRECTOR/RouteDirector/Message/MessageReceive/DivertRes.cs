using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouteDirector
{
	public class DivertRes : MessageBase
	{
		private const Int16 codeStrLen = 22;
		private List<string> ResultCodes = new List<string>{
			"No Dirvert Connect Received",
			"NoRead/Unexpected Tote handled by hardware",
			"Invalid Divert Direction",
			"Divert Jam",
			"Actuator Fault/TimeOut",
			"Lane Full",
			"Overlapping Tracking Windows",
			"Divert Command Received Too Late",
			"Carriers Too Close Together/Congestion",
			"Carton Lost–Tracking Failure",
			"Unsuccessful Divert–Actuator Failure",
			"Carrier Too Long",
			"Length Check Failure",
			"Divert Command Overridden For product Flow Control",
			"Carrier ID Unclear",
		};
		
		private const Int16 messageId = (Int16)MessageBase.MessageType.DivertRes;
		public Int16 nodeId;
		public Int16 cartSeq;
		public Int16 laneId;
		public Int16 divertRes = 0;
		public string codeStr;
		static public int len = 32;

		/// <summary>
		/// 解析消息数组至单个消息对象
		/// </summary>
		/// <param name="buf">消息数组</param>
		/// <param name="offset">数组偏移量</param>
		public DivertRes(byte[] buf, int offset) : base(messageId)
		{
			base.msgBuf = new byte[len];
			Array.Copy(buf, 0, base.msgBuf, 0, len);
			offset += 2;
			offset += DataConversion.ByteToNum(buf, offset, ref nodeId, false);
			offset += DataConversion.ByteToNum(buf, offset, ref cartSeq, false);
			offset += DataConversion.ByteToNum(buf, offset, ref laneId, false);
			offset += DataConversion.ByteToNum(buf, offset, ref divertRes, false);
			codeStr = Encoding.ASCII.GetString(buf, offset, codeStrLen).Trim();
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
			str.AppendLine("laneId = " + laneId.ToString());
			str.AppendLine("divertRes = " + divertRes.ToString());
			str.AppendLine("codeStr = " + codeStr);
			str.AppendLine();
			return str;
		}

		public string GetResult()
		{
			Int16 res = divertRes;
			if (res == 0)
				return "Divert Success";
			string resStr = "Error:";
			int n = 0;
			while (res != 0)
			{
				if ((res & 1) == 1)
				{
					resStr += " ";
					resStr += ResultCodes[n];
					resStr += ",";
				}
				res >>= 1;
				n++;
			}
			return resStr;
		}
	}
}
