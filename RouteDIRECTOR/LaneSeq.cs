using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RouteDirector;
namespace RouteDirector
{
	public class LaneSeq
	{
		public List<Box> boxList = new List<Box> { };
		public Int16 lane = 0;
		public Int16 node;
 
		public LaneSeq(Int16 tLane)
		{
			lane = tLane;
		}

		public void AddBox(Box tBox)
		{
			boxList.Add(tBox);
		}

		public bool HanderReq(Box tBox)
		{
			int index;
			index = boxList.IndexOf(tBox);
			if (index == -1)
				return false;

			int number;
			number = boxList.FindIndex(box => box.status == Box.BoxStatus.Checked);

			if (number == index)
				return true;

			else
			{
				Log.log.Debug("node:" + node + " lane:" + lane + "|next box: No." + number + " " + boxList[number].barcode + "|reject box: NO." + index + " " + tBox.barcode);
				return false;
			}
		}


	}
}
