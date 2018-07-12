using RouteDirector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using System.Threading;
using IRouteDirector;
using System.ServiceModel;

namespace RouteDirector
{
	static class main
	{
		static void Main(string[] args)
		{
			RouteDirectControl routeDirectControl = new RouteDirectControl();
			while (true)
			{
				int res = routeDirectControl.EstablishConnection();
				if (res != 0)
				{
					Log.log.Debug("EstablishConnection fail,try to reconnenct,wiat 10s");
					Thread.Sleep(10000);
				}
				else
					break;
			}
			routeDirectControl.FlushMsg();

			//List<Chest> chestList = new List<Chest> { };
			//chestList.Add(new Chest() { barcode = " 1203", node = 6, lane = 997 });
			//chestList.Add(new Chest() { barcode = "1144", node = 6, lane = 997 });
			//chestList.Add(new Chest() { barcode = "1165", node = 6, lane = 997 });
			//Sorting sorting = new Sorting();
			//if(sorting.AddStackSeq(chestList) != 0)
			//	throw new NotImplementedException();

			ServiceHost host = new ServiceHost(typeof(Sorting));
			host.Open();
			Log.log.Debug("WCF start");

			while (true)
			{
				MessageBase msg;
				msg = routeDirectControl.WaitMsg();

				if (msg.msgId == (Int16)MessageBase.MessageType.DivertReq)
				{
					DivertCmd divertCmd;
					divertCmd = StackSeq.HanderReq((DivertReq)msg);
					if (divertCmd == null)
						routeDirectControl.SendMsg(new HeartBeat(RouteDirectControl.heartBeatTime));
					else
						routeDirectControl.SendMsg(divertCmd);
				}

				if (msg.msgId == (Int16)MessageBase.MessageType.DivertRes)
				{
					StackSeq.HanderRes((DivertRes)msg);
				}

				Log.log.Error("Receive wrong meesage:" + msg.GetInfo(new StringBuilder()));
			}

		}
	}

}
