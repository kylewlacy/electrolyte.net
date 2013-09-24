using System;
using System.Threading;
using InternalTimer = System.Threading.Timer;

namespace Electrolyte.Primitives {
	public class Timer {
		public event EventHandler Elapsed = delegate { };

		public DateTime StartTime { get; private set; }
		public TimeSpan Interval { get; private set; }

		public DateTime EndTime {
			get { return StartTime + Interval; }
		}

		public bool IsRunning { get; private set; }

		protected InternalTimer InternalTimer;

		

		public Timer() {
			InternalTimer = new InternalTimer(new TimerCallback(InternalCallback), null, Timeout.Infinite, Timeout.Infinite);
		}

		public Timer(TimeSpan interval) : this() {
			Interval = interval;
		}

		public void Stop() {
			IsRunning = false;
			InternalTimer.Change(Timeout.Infinite, Timeout.Infinite);
		}

		public void Start() {
			IsRunning = true;
			StartTime = DateTime.Now;
			InternalTimer.Change(Interval, TimeSpan.FromMilliseconds(Timeout.Infinite));
		}

		public void Start(TimeSpan interval) {
			Interval = interval;
			Start();
		}

		protected virtual void InternalCallback(object state) {
			IsRunning = false;
			Elapsed(this, new EventArgs());
		}
	}
}

