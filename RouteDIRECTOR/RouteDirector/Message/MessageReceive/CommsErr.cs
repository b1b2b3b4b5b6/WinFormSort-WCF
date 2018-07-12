using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouteDirector
{
	class CommsErr : MessageBase
	{
		enum Error : Int16
		{
			MsgFormatErr = 1,
			HeartBeatTimeOut = 2,
		}

		static private Int16 messageId = (Int16)MessageType.CommsErr;
		public Int16 error;
		static public int len = 4;

		/// <summary>
		/// 解析消息数组至单个消息对象
		/// </summary>
		/// <param name="buf">消息数组</param>
		/// <param name="offset">数组偏移量</param>
		public CommsErr(byte[] buf, int offset) : base(messageId)
		{
			base.msgBuf = new byte[len];
			Array.Copy(buf, 0, base.msgBuf, 0, len);
			offset += 2;
			offset += DataConversion.ByteToNum(buf, offset, ref error, false);
			
		}

		/// <summary>
		/// 打印消息的参数信息
		/// </summary>
		/// <param name="str">StringBuilder引用</param>
		/// <returns>StringBuilder引用</returns>
		public override StringBuilder GetInfo(StringBuilder str)
		{
			Func<Int16, String> GetName = ((value) =>
			{
				return Enum.GetName(typeof(Error), value);
			});

			str = base.GetInfo(str);
			str.AppendLine("error = " + GetName(error));
			str.AppendLine();
			return str;
		}
	}
}
