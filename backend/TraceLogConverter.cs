using Grpc.Core.Logging;
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
			_log.Debug(logEntry?.ToString());
		}

		public void Debug(object logEntry, Exception e)
		{
			_log.Debug(logEntry?.ToString(), e);
		}

		public void DebugFormat(string formatString, params object[] args)
		{
			_log.Debug(formatString, args);
		}

		public void Error(object logEntry)
		{
			_log.Error(logEntry?.ToString());
		}

		public void Error(object logEntry, Exception e)
		{
			_log.Error(e, logEntry?.ToString());
		}

		public void ErrorFormat(string formatString, params object[] args)
		{
			_log.Error(formatString, args);
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
			_log.Info(logEntry?.ToString());
		}

		public void Info(object logEntry, Exception e)
		{
			_log.Info(logEntry?.ToString(), e);
		}

		public void InfoFormat(string formatString, params object[] args)
		{
			_log.Info(formatString, args);
		}

		public void Warn(object logEntry)
		{
			_log.Warning(logEntry?.ToString());
		}

		public void Warn(object logEntry, Exception e)
		{
			_log.Warning(e, logEntry?.ToString());
		}

		public void WarnFormat(string formatString, params object[] args)
		{
			_log.Warning(formatString, args);
		}
	}
}
