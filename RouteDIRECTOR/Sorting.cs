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
			}
			if (StackSeq.AddStackSeq(boxList) != null)
				return 0;
			else
				return -1;
		}

		public void DleteLastStackSeq()
		{
			throw new NotImplementedException();
		}

		public void Reset()
		{
			throw new NotImplementedException();
		}

		public void ResetWorkingStackSeq()
		{
			throw new NotImplementedException();
		}

		public void StopWorkingStackSeq()
		{
			throw new NotImplementedException();
		}
	}
}
