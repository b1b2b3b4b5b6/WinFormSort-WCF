using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RouteDirector;
namespace IRouteDirector
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
			return StackSeq.AddStackSeq(boxList);
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

		public void ClearAll()
		{
			StackSeq.ClearAll();
		}

		public int ResetWorkingStackSeq()
		{
			return StackSeq.ResetWorkingStackSeq();
		}

		public void StartWorkingStackSeq()
		{
			StackSeq.sortStatus = StackSeq.SortStatus.Working;
			Log.log.Debug("Start sorting");
		}

		public void StopWorkingStackSeq()
		{
			StackSeq.sortStatus = StackSeq.SortStatus.Stoping;
			Log.log.Debug("Stop sorting");
		}

		public int GetWaitSeqStackCount()
		{
			return StackSeq.waitingStackSeqList.Count();
		}

	}
}
