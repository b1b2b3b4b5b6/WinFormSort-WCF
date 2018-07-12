using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace RouteDirector
{
	public static class Log
	{
		public static ILog log = LogManager.GetLogger("RouteDirector");
	}
}
