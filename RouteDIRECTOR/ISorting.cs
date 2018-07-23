using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace IRouteDirector
{
	// 未注明返回值的函数默认成功返回0，失败返回-1；
	[ServiceContract] 
	public interface ISorting
	{
		[OperationContract]
		int AddStackSeq(string strJson); //添加分拣序列（托盘号），List<Chest>序列化

		[OperationContract]
		int ClearAll(); //清除所有分拣任务，初始化分拣机

		[OperationContract]
		int ResetWorkingStackSeq(); //重置正在进行分拣的序列（托盘号），使其进入等待分拣状态

		[OperationContract]
		int DeleteStackSeq(int tSeqNum); //删除分拣序列，参数为托盘号

		[OperationContract]
		int StopWorkingStackSeq(); //停止分拣（默认状态）

		[OperationContract]
		int StartWorkingStackSeq(); //启动分拣

		[OperationContract]
		int GetWorkingStackSeq(); //返回正在进行分拣的序列号（托盘号）

		[OperationContract]
		int GetWaitSeqStackCount(); //返回正在等待分拣任务数

	}

	public class Chest
	{
		public string barcode;
		public Int16 node;
		public Int16 lane;
		public int container;
	}

	
}
		
