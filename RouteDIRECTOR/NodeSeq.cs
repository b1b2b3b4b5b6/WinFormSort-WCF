using RouteDirector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RouteDirector
{
	public class NodeSeq
	{
		public Int16 node;
		public List<LaneSeq> laneSeqList = new List<LaneSeq> { };
		public NodeSeq(Int16 tNode)
		{
			node = tNode;
		}

		public NodeSeq(NodeSeq tNodeSeq)
		{
			node = tNodeSeq.node;
			laneSeqList = tNodeSeq.laneSeqList;
		}
		public void AddBox(Box tBox)
		{
			int index;
			index = laneSeqList.FindIndex(laneSeq => laneSeq.lane == tBox.lane);
			if (index != -1)
				laneSeqList[index].AddBox(tBox);
			else
			{
				LaneSeq laneSeq = new LaneSeq(tBox.lane);
				laneSeq.node = node;
				laneSeqList.Add(laneSeq);
				laneSeq.AddBox(tBox);
			}
		}

		public bool HanderReq(Box tBox)
		{
			foreach (LaneSeq laneSeq in laneSeqList)
			{
				if (laneSeq.HanderReq(tBox))
					return true;
			}
			return false;
		}

	}
}
