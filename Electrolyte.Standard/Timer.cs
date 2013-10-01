using System;
using System.Threading;
using Tiko;
using AbstractTimer = Electrolyte.Portable.Timer;
using InternalTimer = System.Threading.Timer;

namespace Electrolyte.Standard {
	[Resolves(typeof(AbstractTimer))]
	public class Timer : AbstractTimer {
		protected InternalTimer InternalTimer;

		protected override void Initialize() {
			InternalTimer = new InternalTimer(new TimerCallback(InternalCallback), null, Timeout.Infinite, Timeout.Infinite);
		}

		protected override void Initialize(TimeSpan interval) {
			Initialize();
			Interval = interval;
		}

		public override void Start() {
			IsRunning = true;
			StartTime = DateTime.Now;
			InternalTimer.Change(Interval, TimeSpan.FromMilliseconds(Timeout.Infinite));
		}

		public override void Stop() {
			IsRunning = false;
			InternalTimer.Change(Timeout.Infinite, Timeout.Infinite);
		}
	}
}

