using Grpc.Core.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace plugin_dotnet
{
	public interface ISubscriptionReaper : IDisposable
	{
		event EventHandler<EventArgs> OnTimer;
		void Start();

		void Stop();
	}

	public class SubscriptionReaper : ISubscriptionReaper
	{
		private ILogger _logger;
		private Timer _reaperTimer;
		private int _timerInterval; 
		public SubscriptionReaper(ILogger logger, int timerInterval)
		{
			_logger = logger;
			_timerInterval = timerInterval;
			_reaperTimer = new Timer(state => DoWork());
		}

		public event EventHandler<EventArgs> OnTimer;

		public void Dispose()
		{
			_reaperTimer.Dispose();
		}

		public void Start()
		{
			_logger.Debug("Starting timer with interval [ms]: {0}", _timerInterval);
			_reaperTimer.Change(0, int.MaxValue);
		}

		public void Stop()
		{
			_logger.Debug("Stopping timer with interval [ms]: {0}", _timerInterval);
			_reaperTimer.Change(int.MaxValue, int.MaxValue);
		}

		private void DoWork()
		{
			try
			{
				OnTimer?.Invoke(this, EventArgs.Empty);
			}
			catch (Exception e)
			{
				_logger.Error(e, "Error in timer invoke");
			}
			_reaperTimer.Change(_timerInterval, int.MaxValue);
		}
	}
}
