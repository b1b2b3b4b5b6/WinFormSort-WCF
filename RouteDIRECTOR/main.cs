using RouteDirector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using System.Threading;
using System.ServiceModel;

namespace RouteDirector
{
	static class main
	{
		static void Main(string[] args)
		{
			RouteDirectControl routeDirectControl = new RouteDirectControl();
			routeDirectControl.ContinueConnection();
			routeDirectControl.FlushMsg();
			StackSeq.Init();

			ServiceHost host = new ServiceHost(typeof(Sorting));
			host.Open();
			Log.log.Info("WCF start");

			while (true)
			{
				MessageBase msg;
				msg = routeDirectControl.WaitMsg();
				DivertCmd divertCmd = StackSeq.Hander(msg);
				if (divertCmd != null)
					routeDirectControl.SendMsg(divertCmd); 
			}

		}
	}

}
