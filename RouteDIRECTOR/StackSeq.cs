using System;
using System.Collections.Generic;
using static RouteDirector.Box;
using IRouteDirector;
namespace RouteDirector
{
	public class StackSeq
	{
		public enum SortStatus
		{
			Working = 0,
			Stoping,
		}
		static public SortStatus sortStatus = SortStatus.Stoping;
		static public StackSeq workingStack = null;
		static public List<StackSeq> waitingStackSeqList = new List<StackSeq> { };
		static private List<string> outListBox = new List<string> { };
		static readonly object lockObject = new object();
		static System.Timers.Timer recHeartTime;
		static System.Timers.Timer sendHeartTime;
		public enum StackStatus
		{
			Finished = 0,
			CheckingBox,
			BoxChecked,
			Busying,
			Inital,
		}
		public StackStatus stackStatus = StackStatus.Inital;
		public int SeqNum = 0;
		public List<Box> boxList = new List<Box> { };
		public List<NodeSeq> nodeSeqList = new List<NodeSeq> { };

		static Int16 solveNode = 6;
		static Int16 solveLane = 997;

		static public int AddStackSeq(List<Box> tBoxList, int container)
		{
			StackSeq stackSeq = new StackSeq();
			int res = stackSeq.AddBoxList(tBoxList);
			if (res <= 0)
				Log.log.Info("StackSeq add fail");
			else
			{
				Log.log.Info("Added stacklist, box counts is " + stackSeq.boxList.Count + ", seqnum is " + container);
				foreach (Box box in stackSeq.boxList)
				{
					Log.log.Info("barcode:" + box.barcode + "|node:" + box.node + "|lane" + box.lane);
				}
				waitingStackSeqList.Add(stackSeq);
				stackSeq.SeqNum = container;
				return stackSeq.SeqNum;
			}
			return -1;
		}

		static public int DeleteStackSeq(int tSeqNum)
		{
			lock (lockObject)
			{
				if (workingStack != null)
				{
					if (tSeqNum == workingStack.SeqNum)
					{
						workingStack = null;
						Log.log.Info("Delete stackseq[" + tSeqNum + "] success");
						return 0;
					}
				}

				StackSeq stackSeq;
				stackSeq = waitingStackSeqList.Find(mStackSeq => mStackSeq.SeqNum == tSeqNum);
				if (stackSeq != null)
				{
					waitingStackSeqList.Remove(stackSeq);
					Log.log.Info("Delete stackseq[" + tSeqNum + "] success");
					return 0;
				}
			}
			Log.log.Info("stackseq[" + tSeqNum + "] not exist");
			return -1;
		}

		static public void ClearAll()
		{
			lock (lockObject)
			{
				workingStack = null;
				waitingStackSeqList.Clear();
				outListBox.Clear();
				sortStatus = SortStatus.Stoping;
				Log.log.Info("Clear all");
			}
		}

		static public int ResetWorkingStackSeq()
		{
			if (workingStack == null)
			{
				Log.log.Info("Delet working stackseq fail, There is no working stack");
				return -1;
			}

			lock (lockObject)
			{
				workingStack.boxList.ForEach(box => box.status = BoxStatus.Inital);
				workingStack.stackStatus = StackStatus.Inital;
				waitingStackSeqList.Insert(0, workingStack);
				workingStack = null;
				Log.log.Info("Reset working stack");
				return 0;
			}
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

		static public DivertCmd Hander(MessageBase msg)
		{
			lock (lockObject)
			{
				if (msg.msgId == (Int16)MessageBase.MessageType.DivertReq)
					return StackSeq.HanderReq((DivertReq)msg);

				if (msg.msgId == (Int16)MessageBase.MessageType.DivertRes)
					StackSeq.HanderRes((DivertRes)msg);
				return null;
			}

		}

		static private DivertCmd HanderReq(DivertReq divertReq)
		{
			if (sortStatus == SortStatus.Stoping)
				return null;
			Box box;
			if (divertReq.codeStr.Contains("?"))
			{
				Log.log.Info("find unknow box in node:" + divertReq.nodeId);
				if (divertReq.nodeId == solveNode)
				{
					Log.log.Info("removing unknow box from line");
					return new DivertCmd(divertReq, solveLane);
				}
				else
					Log.log.Info("Will remove unknow box from line in solve node[" + solveLane +"]");
			}

			if (CheckStackSeq() == true)
			{
				box = FindBox(workingStack, divertReq.codeStr);
				if (box != null)
				{
					foreach (NodeSeq nodeSeq in workingStack.nodeSeqList)
					{
						if (nodeSeq.node == divertReq.nodeId)
						{
							if (nodeSeq.HanderReq(box) == true)
							{
								Log.log.Info("Box: " + box.barcode + " is sorting");
								box.status = BoxStatus.Sorting;
								return new DivertCmd(divertReq, box.lane);
							}
						}
					}
					return null;
				}
			}


			foreach (StackSeq stackSeq in waitingStackSeqList)
			{
				box = FindBox(stackSeq, divertReq.codeStr);
				if (box != null)
				{
					if (box.status == BoxStatus.Inital)
					{
						box.status = BoxStatus.Checked;
						Log.log.Info("Seq["+ stackSeq.SeqNum + "] Box["+ box.barcode+"]"+ " has been checked");
						if (stackSeq.boxList.TrueForAll(mBox => mBox.status == BoxStatus.Checked) == true)
						{
							Log.log.Info("Seq[" + stackSeq.SeqNum + "] all box has been checked");
							stackSeq.stackStatus = StackStatus.BoxChecked;
						}
					}
					return null;
				}
			}

			Log.log.Info("find box["+ divertReq.codeStr + "] out of list");
			if (divertReq.nodeId == solveNode)
			{
				Log.log.Info("removing outlist box[" + divertReq.codeStr + "] from line");
				outListBox.Add(divertReq.codeStr);
				return new DivertCmd(divertReq, solveLane);
			}
			else
				Log.log.Info("Will remove outlist box[" + divertReq.codeStr + "] from line in solve node[" + solveLane + "]");
			return  null;
		}

		static private void HanderRes(DivertRes divertRes)
		{
			if (divertRes.divertRes == 1)
				return;
			Box box;
			if (divertRes.codeStr.Contains("?")&&(divertRes.nodeId == solveNode))
			{
				if (divertRes.divertRes == 0)
					Log.log.Info("Success removed unknow box from line");
				else
				{
					Log.log.Info("removed unknow box from conveyor line fail: "+divertRes.GetResult());
					;
				}
				return;
			}
			if (CheckStackSeq() == true)
			{
				box = FindBox(workingStack, divertRes.codeStr);
				if (box != null)
				{

					if (box.status == BoxStatus.Sorting)
					{
						if (divertRes.divertRes == 0)
						{
							box.status = BoxStatus.Success;
							Log.log.Info("Box[" + box.barcode + "] sort success");
							workingStack.CheckStatus();
							ReportBox(new Chest() { barcode = box.barcode, lane = box.lane, node = box.node, container = workingStack.SeqNum });
						}
						else
						{
							box.status = BoxStatus.Checked;
							if (divertRes.divertRes == 32)
							{
								Log.log.Error("box["+box.barcode+"] sort faild,node["+ divertRes.nodeId + "]"+ "lane["+ box.lane + "] is full");
								//ReportStatus(StatusToReport.LaneFull);
								return;
							}
				
							Log.log.Error("Box[" + box.barcode + "] sorting falut: " + divertRes.GetResult());
							Log.log.Error("ahead[node:" + box.node + "|lane:" + box.lane + "] now[node:" + divertRes.nodeId + "|lane:" + divertRes.laneId + "]");
							ReportError(new ErrorInfo(ErrorInfo.ErrorCode.SortingFault, box.barcode, box.node, box.lane));
						}
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
			string barcode = outListBox.Find(mBarcode => mBarcode.Equals(divertRes.codeStr));
			if (barcode != null)
			{ 
				outListBox.Remove(barcode);
				if (divertRes.divertRes == 0)
					Log.log.Info("Box[" + divertRes.codeStr + "] out of list sort success");
				else
				{
					Log.log.Error("Box[" + divertRes.codeStr + "] out of list sorting falut: " + divertRes.GetResult());
					Log.log.Error("ahead[node:" + solveNode + "|lane:" + solveLane + "] now[node:" + divertRes.nodeId + "|lane:" + divertRes.laneId + "]");
					ReportError(new ErrorInfo(ErrorInfo.ErrorCode.SortingFault, barcode, solveNode, solveLane));
				}
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

			if ((workingStack != null))
			{
				if(workingStack.stackStatus == StackStatus.Finished)
				workingStack = null;
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
						Log.log.Info("WorkStack is empty,set waiting stackseq to work");
						return true;
					}
				}
			}
			return false;
		}

		private void CheckStatus()
		{
			if (boxList.TrueForAll(box => box.status == BoxStatus.Success) == true)
			{
				Log.log.Info("stack sort success");
				stackStatus = StackStatus.Finished;
				ReportContainer(SeqNum);
				CheckStackSeq();
			}
		}

		private class ErrorInfo
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
		}
		
		static private void ReportContainer(int container)
		{

		}

		static private void ReportError(ErrorInfo errorInfo)
		{

		}

		static private void ReportBox(Chest chest)
		{

		}

		#region
	
		#endregion
	}
}
