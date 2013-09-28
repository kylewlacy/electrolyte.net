using System;
using Tiko;

namespace Electrolyte.Portable {
	public abstract class Timer {
		public event EventHandler Elapsed = delegate { };

		public virtual DateTime StartTime { get; protected set; }
		public virtual TimeSpan Interval { get; protected set; }

		public virtual DateTime EndTime {
			get { return StartTime + Interval; }
		}

		public virtual bool IsRunning { get; protected set; }

		

		protected abstract void Initialize();
		protected abstract void Initialize(TimeSpan interval);

		public abstract void Start();
		public virtual void Start(TimeSpan interval) {
			Interval = interval;
			Start();
		}

		public abstract void Stop();

		protected virtual void InternalCallback(object state) {
			IsRunning = false;
			Elapsed(this, new EventArgs());
		}

		public static Timer Create() {
			var timer = TikoContainer.Resolve<Timer>();
			timer.Initialize();
			return timer;
		}

		public static Timer Create(TimeSpan interval) {
			var timer = TikoContainer.Resolve<Timer>();
			timer.Initialize(interval);
			return timer;
		}
	}
}

