using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RouteDirector
{
	public class Box
	{
		public enum BoxStatus
		{
			Success = 0,
			SortFail,
			Checked,
			Losing,
			OutList,
			Sorting,
			Inital,
		}

		public string barcode;
		public Int16 node;
		public Int16 lane;
		public BoxStatus status;
		public int showTimes;
		public Box()
		{
			barcode = "";
			node = 0;
			lane = 0;
			status = BoxStatus.Inital;
			showTimes = 0;
		}
		public Box(Box box)
		{
			barcode = box.barcode;
			node = box.node;
			lane = box.lane;
			status = box.status;
			showTimes = box.showTimes;
		}
	};
}
