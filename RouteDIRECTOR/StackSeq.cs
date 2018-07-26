using System;
using System.Collections.Generic;
using static RouteDirector.Box;
using static RouteDirector.ReportInfo;
namespace RouteDirector
{
	public class StackSeq
	{
		public enum SortStatus
		{
			Working = 0,
			Stoping,
		}
		public static SortStatus sortStatus;
		public static StackSeq workingStack = null;
		public static  List<StackSeq> waitingStackSeqList;
		private static List<string> outListBox;
		private static readonly object lockObject = new object();
		private static System.Timers.Timer sortingTimer;
		private static int sortingTime = 30;

		public enum StackStatus
		{
			Finished = 0,
			CheckingBox,
			BoxChecked,
			Sorting,
			Inital,
		}
		public StackStatus stackStatus = StackStatus.Inital;
		public int SeqNum;
		public List<Box> boxList = new List<Box> { };
		public List<NodeSeq> nodeSeqList = new List<NodeSeq> { };

		static Int16 solveNode = 6;
		static Int16 solveLane = 997;
		public StackSeq()
		{
			boxList.Clear();
			nodeSeqList.Clear();
			SeqNum = 0;
		}

		private int AddBoxList(List<Box> tBoxList)
		{

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

		private void CheckStatus()
		{
			if (boxList.TrueForAll(box => box.status == BoxStatus.Success) == true)
			{
				Log.log.Info("stack sort success");
				stackStatus = StackStatus.Finished;
				SortingTimerStop();
				ReportContainer(SeqNum);
				CheckStackSeq();
			}
		}

		private string GetNextBox(BoxStatus boxStatus)
		{
			Box box = boxList.Find(mBox => mBox.status != boxStatus);
			if (box == null)
				return null;
			else
				return box.barcode;
		}

		static public void Init()
		{

			SortingTimerInit(sortingTime);
			sortStatus = SortStatus.Stoping;
			workingStack = null;
			waitingStackSeqList = new List<StackSeq> { };
			outListBox = new List<string> { };
		}

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
				CheckStackSeq();
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
						SortingTimerStop();
						return 0;
					}
				}

				StackSeq stackSeq;
				stackSeq = waitingStackSeqList.Find(mStackSeq => mStackSeq.SeqNum == tSeqNum);
				if (stackSeq != null)
				{
					if (waitingStackSeqList.IndexOf(stackSeq) == 0)
						SortingTimerStop();
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
				SortingTimerStop();
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
				SortingTimerStop();
				Log.log.Info("Reset working stack");
				return 0;
			}
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
					if (waitingStackSeqList.IndexOf(stackSeq) != 0)
						return null;
					if (box.status == BoxStatus.Inital)
					{
						box.status = BoxStatus.Checked;
						Log.log.Info("Seq["+ stackSeq.SeqNum + "] Box["+ box.barcode+"]"+ " has been checked");
						SortingTimeReset();
						if (stackSeq.boxList.TrueForAll(mBox => mBox.status == BoxStatus.Checked) == true)
						{
							Log.log.Info("Seq[" + stackSeq.SeqNum + "] all box has been checked");
							SortingTimerStop();
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
					Log.log.Info("removed unknow box from conveyor line fail: " + divertRes.GetResult());
					if(divertRes.divertRes == 32)
						ReportError(new ErrorInfo(ErrorInfo.ErrorCode.LaneFull, "?", solveNode, solveLane));
					else
						ReportError(new ErrorInfo(ErrorInfo.ErrorCode.SortingFault, "?", solveNode, solveLane));
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
							SortingTimeReset();
							workingStack.CheckStatus();
							ReportBox(new Chest() { barcode = box.barcode, lane = box.lane, node = box.node, container = workingStack.SeqNum });
						}
						else
						{
							box.status = BoxStatus.Checked;
							if (divertRes.divertRes == 32)
							{
								Log.log.Error("box["+box.barcode+"] sort faild,node["+ divertRes.nodeId + "]"+ "lane["+ box.lane + "] is full");
								ReportError(new ErrorInfo(ErrorInfo.ErrorCode.LaneFull, box.barcode, box.node, box.lane));
								return;
							}
				
							Log.log.Error("Box[" + box.barcode + "] sorting falut: " + divertRes.GetResult());
							Log.log.Error("ahead[node:" + box.node + "|lane:" + box.lane + "] now[node:" + divertRes.nodeId + "|lane:" + divertRes.laneId + "]");
							ReportError(new ErrorInfo(ErrorInfo.ErrorCode.SortingFault, box.barcode, box.node, box.lane));
						}
					}
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
			{
				CheckTimeout();
				return true;
			}
			else
			{
				if (waitingStackSeqList.Count > 0)
				{
					if (waitingStackSeqList[0].stackStatus == StackStatus.BoxChecked)
					{
						workingStack = waitingStackSeqList[0];
						workingStack.stackStatus = StackStatus.Sorting;
						waitingStackSeqList.RemoveAt(0);
						Log.log.Info("WorkStack is empty,set waiting stackseq to work");
						CheckTimeout();
						return true;
					}
				}
			}
			CheckTimeout();
			return false;
		}

		static private void CheckTimeout()
		{
			if (workingStack != null)
				SortingTimerStart();
			else
			{
				if(waitingStackSeqList.Count > 0)
					SortingTimerStart();
			}
		}

		static public void Start()
		{
			Log.log.Info("Start sorting");
			sortStatus = SortStatus.Working;
			CheckStackSeq();
			
		}

		static public void Stop()
		{
			Log.log.Info("Stop sorting");
			sortStatus = SortStatus.Stoping;
			SortingTimerStop();
		}

		#region timeout check

		static private void SortingTimerInit(int s)
		{
			Log.log.Debug("Sorting heartbeat init");
			sortingTimer = new System.Timers.Timer();
			sortingTimer.Elapsed += SortingTimer_Elapsed;
			sortingTimer.Interval = s * 1000;
			sortingTimer.AutoReset = false;
			sortingTimer.Stop();
		}

		static private void SortingTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			Log.log.Debug("Sorting timer elapsed");
			if (workingStack != null)
				ReportError(new ErrorInfo(ErrorInfo.ErrorCode.SortingTimeout, workingStack.GetNextBox(BoxStatus.Checked), 0, 0));
			else
				ReportError(new ErrorInfo(ErrorInfo.ErrorCode.CheckTimeout, waitingStackSeqList[0].GetNextBox(BoxStatus.Inital), 0, 0));
			SortingTimeReset();
		}

		static private void SortingTimerStart()
		{

			if (sortingTimer.Enabled == true)
				return;
			else
			{
				sortingTimer.Start();
				Log.log.Debug("Sorting timer start");
			}
		}

		static private void SortingTimerStop()
		{
			sortingTimer.Stop();
			Log.log.Debug("Sorting timer stop");
		}

		static private void SortingTimeReset()
		{
			sortingTimer.Stop();
			sortingTimer.Start();
			Log.log.Debug("Sorting timer reset");
		}
		#endregion
	}
}
