using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace DontShaveYourHead
{
	public interface ILogger
	{
		void LogMessage(string message);
	}

	public class Logger : ILogger
	{
		public void LogMessage(string message)
		{
			Log.Message($"DSYH: {message}");
		}
	}

	public class Logger_Nothing : ILogger
	{
		public void LogMessage(string message){}
	}
}
