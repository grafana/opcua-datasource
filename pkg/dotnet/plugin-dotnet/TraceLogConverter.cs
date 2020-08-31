using Microsoft.Extensions.Logging;
using Prediktor.Log;
using System;
using System.Collections.Generic;
using System.Text;

namespace plugin_dotnet
{
	public class TraceLogConverter : ITraceLog
	{
		private ILogger _log;
		public TraceLogConverter(ILogger log)
		{
			_log = log;
		}
		public bool IsDebugEnabled => true;

		public bool IsInfoEnabled => true;

		public bool IsWarnEnabled => true;

		public bool IsErrorEnabled => true;

		public bool IsFatalEnabled => true;

		public void Debug(object logEntry)
		{
			_log.LogDebug(logEntry?.ToString());
		}

		public void Debug(object logEntry, Exception e)
		{
			_log.LogDebug(e, logEntry?.ToString());
		}

		public void DebugFormat(string formatString, params object[] args)
		{
			_log.LogDebug(formatString, args);
		}

		public void Error(object logEntry)
		{
			_log.LogError(logEntry?.ToString());
		}

		public void Error(object logEntry, Exception e)
		{
			_log.LogError(e, logEntry?.ToString());
		}

		public void ErrorFormat(string formatString, params object[] args)
		{
			_log.LogError(formatString, args);
		}

		public void Fatal(object logEntry)
		{
			Error(logEntry);
		}

		public void Fatal(object logEntry, Exception e)
		{
			Error(logEntry, e);
		}

		public void FatalFormat(string formatString, params object[] args)
		{
			ErrorFormat(formatString, args);
		}

		public void Info(object logEntry)
		{
			_log.LogInformation(logEntry?.ToString());
		}

		public void Info(object logEntry, Exception e)
		{
			_log.LogInformation(e, logEntry?.ToString());
		}

		public void InfoFormat(string formatString, params object[] args)
		{
			_log.LogInformation(formatString, args);
		}

		public void Warn(object logEntry)
		{
			_log.LogWarning(logEntry?.ToString());
		}

		public void Warn(object logEntry, Exception e)
		{
			_log.LogWarning(e, logEntry?.ToString());
		}

		public void WarnFormat(string formatString, params object[] args)
		{
			_log.LogWarning(formatString, args);
		}
	}
}
