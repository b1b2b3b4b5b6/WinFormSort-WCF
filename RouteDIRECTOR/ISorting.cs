using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace IRouteDirector
{
	// 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码和配置文件中的接口名“ISorting”。
	[ServiceContract]
	public interface ISorting
	{
		[OperationContract]
		int AddStackSeq(string strJson);

		[OperationContract]
		void ClearAll();

		[OperationContract]
		int ResetWorkingStackSeq();

		[OperationContract]
		int DleteStackSeq(int tSeqNum);
		
		[OperationContract]
		void StopWorkingStackSeq();

		[OperationContract]
		void StartWorkingStackSeq();

		[OperationContract]
		int GetWorkingStackSeq();
	}

	[DataContract]
	public class Chest
	{
		[DataMember]
		public string barcode;
		[DataMember]
		public Int16 node;
		[DataMember]
		public Int16 lane;
	}

	
}
		
