using System;

namespace Electrolyte.Portable {
	public abstract class Timer {
		public event EventHandler Elapsed = delegate { };

		public virtual DateTime StartTime { get; private set; }
		public virtual TimeSpan Interval { get; private set; }

		public virtual DateTime EndTime {
			get { return StartTime + Interval; }
		}

		public virtual bool IsRunning { get; private set; }

		

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
			throw new NotImplementedException();
		}

		public static Timer Create(TimeSpan interval) {
			throw new NotImplementedException();
		}
	}
}

