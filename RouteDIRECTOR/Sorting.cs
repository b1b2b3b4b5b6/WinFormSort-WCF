using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RouteDirector
{
	public class Sorting : ISorting
	{

		public int AddStackSeq(string strJson)
		{
			List<Chest> chestList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Chest>>(strJson);
			List<Box> boxList = new List<Box> { };
			foreach (Chest chest in chestList)
			{
				Box box = new Box();
				box.barcode = chest.barcode;
				box.lane = chest.lane;
				box.node = chest.node;
				box.status = Box.BoxStatus.Inital;
				boxList.Add(box);
			}
			return StackSeq.AddStackSeq(boxList, chestList[0].container);
		}

		public int DeleteStackSeq(int tSeqNum)
		{
			return StackSeq.DeleteStackSeq(tSeqNum);
		}

		public int GetWorkingStackSeq()
		{
			if (StackSeq.workingStack != null)
				return StackSeq.workingStack.SeqNum;
			else
				return -1;
		}

		public int ClearAll()
		{
			StackSeq.ClearAll();
			return 0;
		}

		public int ResetWorkingStackSeq()
		{
			return StackSeq.ResetWorkingStackSeq();
		}

		public int StartWorkingStackSeq()
		{
			StackSeq.sortStatus = StackSeq.SortStatus.Working;
			Log.log.Info("Start sorting");
			return 0;
		}

		public int StopWorkingStackSeq()
		{
			StackSeq.sortStatus = StackSeq.SortStatus.Stoping;
			Log.log.Info("Stop sorting");
			return 0;
		}

		public int GetWaitSeqStackCount()
		{
			return StackSeq.waitingStackSeqList.Count();
		}

	}

}
