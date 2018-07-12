using System;
using System.Collections.Generic;
using static RouteDirector.Box;
using IRouteDirector;
namespace RouteDirector
{
	public class StackSeq
	{
		public enum StackStatus
		{
			Finished = 0,
			CheckingBox,
			BoxChecked,
			LosingBox,
			SortFalut,
			RobotFalut,
			Busying,
			Inital,
		}
		public StackStatus stackStatus = StackStatus.Inital;

		static public StackSeq workingStack = null;
		static public List<StackSeq> waitingStackSeqList =  new List<StackSeq>{};

		public List<Box> boxList = new List<Box> { };
		public List<NodeSeq> nodeSeqList = new List<NodeSeq> { };

		static Int16 solveNode = 6;
		static Int16 solveLane = 997;

		static public StackSeq AddStackSeq(List<Box> tBoxList)
		{
			StackSeq stackSeq = new StackSeq();
			int res = stackSeq.AddBoxList(tBoxList);
			if (res <= 0)
				Log.log.Debug("StackSeq add fail");
			else
			{
				Log.log.Debug("Added stacklist, box count is " + stackSeq. boxList.Count);
				foreach (Box box in stackSeq.boxList)
				{
					Log.log.Debug("barcode:" + box.barcode + "|node:" + box.node + "|lane" + box.lane);
				}
				waitingStackSeqList.Add(stackSeq);
				return stackSeq;
			}
			return null;
		}

		private int AddBoxList(List<Box> tBoxList)
		{
			boxList.Clear();
			nodeSeqList.Clear();
			foreach (Box box in tBoxList)
			{
				boxList.Add(box);
				int index;
				index = nodeSeqList.FindIndex(mNodeSeq => mNodeSeq.node == box.node);
				if (index == -1)
				{
					NodeSeq nodeSeq = new NodeSeq(box.node);
					nodeSeqList.Add(nodeSeq);
					nodeSeq.AddBox(box);
				}
				else
					nodeSeqList[index].AddBox(box);
			}

			stackStatus = StackStatus.CheckingBox;

			return boxList.Count;
		}

		static public DivertCmd HanderReq(DivertReq divertReq)
		{
			if (divertReq.codeStr.Contains("?"))
			{
				Log.log.Debug("find unknow box");
				if (divertReq.nodeId == solveNode)
				{
					Log.log.Debug("removing unknow box from conveyor line");
					return new DivertCmd(divertReq, solveLane);
				}
				else
					Log.log.Debug("Can not remove unknow box from conveyor line, it is not in solve node");
			}
			
			Box box = FindBox(workingStack, divertReq.codeStr);
			if (box != null)
			{
				foreach (NodeSeq nodeSeq in workingStack.nodeSeqList)
				{
					if (nodeSeq.node == divertReq.nodeId)
					{
						if (nodeSeq.HanderReq(box) == true)
						{
							Log.log.Debug("Box: " + box.barcode + " is sorting");
							box.status = BoxStatus.Sorting;
							return new DivertCmd(divertReq, box.lane);
						}
					}
				}
				return null;
			}

			foreach (StackSeq stackSeq in waitingStackSeqList)
			{
				box = FindBox(stackSeq, divertReq.codeStr);
				if (box != null)
				{
					if (box.status == BoxStatus.Inital)
					{
						box.status = BoxStatus.Checked;
						if (stackSeq.boxList.TrueForAll(mBox => mBox.status == BoxStatus.Checked) == true)
							stackSeq.stackStatus = StackStatus.BoxChecked;
						
					}
					return null;
				}
			}

			Log.log.Debug("find box["+ divertReq.codeStr + "] out of list");
			if (divertReq.nodeId == solveNode)
			{
				Log.log.Debug("removing outlist box[" + divertReq.codeStr + "] from conveyor line");
				return new DivertCmd(divertReq, solveLane);
			}
			else
				Log.log.Debug("Can not remove outlist box[" + divertReq.codeStr + "] from conveyor line, it is not in solve node");
			return  null;
		}

		static public void HanderRes(DivertRes divertRes)
		{
			if (divertRes.codeStr.Contains("?"))
			{
				Log.log.Debug("Success removed unknow box from conveyor line");
				return;
			}

			Box box = FindBox(workingStack, divertRes.codeStr);
			if (box != null)
			{
				if (box.status == BoxStatus.Sorting)
				{
					if ((box.node == divertRes.nodeId) && (box.lane == divertRes.laneId))
					{
						box.status = BoxStatus.Success;
						Log.log.Debug("Box[" + box.barcode + "] sort success");
						workingStack.CheckStatus();
					}
					else
					{
						Log.log.Error("Box[" + box.barcode + "] sorting falut: " + divertRes.GetResult());
						Log.log.Debug("ahead[node:" + box.node + "|lane:" + box.lane + "] now[node:" + divertRes.nodeId + "|lane:" + divertRes.laneId+"]");
						workingStack.stackStatus = StackStatus.SortFalut;
						throw new Exception("Box[" + box.barcode + "] sorting fail");
					}
				}
			}

			foreach (StackSeq stackSeq in waitingStackSeqList)
			{
				box = FindBox(stackSeq, divertRes.codeStr);
				if (box != null)
				{
					return;
				}
			}

			if ((solveNode == divertRes.nodeId) && (solveLane == divertRes.laneId))
				Log.log.Debug("Box[" + divertRes.codeStr + "] out of list sort success");
			else
			{
				Log.log.Error("Box[" + box.barcode + "] out of list sorting falut: " + divertRes.GetResult());
				Log.log.Debug("ahead[node:" + solveNode + "|lane:" + solveLane + "] now[node:" + divertRes.nodeId + "|lane:" + divertRes.laneId + "]");
				throw new Exception("Box[" + box.barcode + "] out of list sorting fail");
			}
		}

		static private Box FindBox(StackSeq tStackSeq, string barcode)
		{
			int index = tStackSeq.boxList.FindIndex(box => box.barcode.Equals(barcode));
			if (index != -1)
				return tStackSeq.boxList[index];
			else
				return null;
		}

		static private bool CheckStackSeq()
		{
			if (workingStack != null)
			{
				if (workingStack.stackStatus == StackStatus.Finished)
				{
					SendFinish();
					workingStack = null;
				}
			}

			if (workingStack != null)
				return true;
			else
			{
				if (waitingStackSeqList.Count > 0)
				{
					if (waitingStackSeqList[0].stackStatus == StackStatus.BoxChecked)
					{
						workingStack = waitingStackSeqList[0];
						workingStack.stackStatus = StackStatus.Busying;
						waitingStackSeqList.RemoveAt(0);
						Log.log.Debug("WorkStack is empty,set waiting stackseq to work");
						return true;
					}
				}
			}
			return false;
		}

		static void SendFinish()
		{ }

		private void CheckStatus()
		{
			if (boxList.TrueForAll(box => box.status == BoxStatus.Success) == true)
			{
				Log.log.Debug("stack sort success");
				stackStatus = StackStatus.Finished;
			}
		}
	}
}
