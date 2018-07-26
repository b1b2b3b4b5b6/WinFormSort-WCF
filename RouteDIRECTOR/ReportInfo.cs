using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RouteDirector
{
	public static class ReportInfo
	{
		public class ErrorInfo
		{
			public enum ErrorCode
			{
				Unknow = 1,
				Outlist,
				CheckTimeout,
				SortingTimeout,
				LaneFull,
				SortingFault,
				ConnectionFalut
			}

			public ErrorCode errorCode;
			public string barcode;
			public Int16 node;
			public Int16 lane;

			public ErrorInfo(ErrorCode tErrorCode, string tBarcode, Int16 tNode, Int16 tLane)
			{
				errorCode = tErrorCode;
				barcode = tBarcode;
				node = tNode;
				lane = tLane;
			}

			public ErrorInfo(ErrorCode tErrorCode)
			{
				errorCode = tErrorCode;
			}
		}

		public static void ReportContainer(int container)
		{
			Log.log.Info(container);
		}

		public static void ReportError(ErrorInfo errorInfo)
		{
			Func<int, String> GetName = ((value) => {
				return Enum.GetName(typeof(ErrorInfo.ErrorCode), value);
			});
			Log.log.Info("Report error: " + GetName((int)errorInfo.errorCode));
		}

		public static void ReportBox(Chest chest)
		{
			Log.log.Info("Report box: " + chest.barcode);
		}
	}
}
