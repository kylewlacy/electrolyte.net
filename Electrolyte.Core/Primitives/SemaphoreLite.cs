using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Electrolyte.Primitives {
	// http://blogs.msdn.com/b/pfxteam/archive/2012/02/12/10266983.aspx
	public class SemaphoreLite {
		public int CurrentCount { get; private set; }
		event EventHandler OnRelease = delegate { };
		readonly Queue<TaskCompletionSource<bool>> Waiters = new Queue<TaskCompletionSource<bool>>();

		public SemaphoreLite(int initialCount = 1) {
			CurrentCount = initialCount;
		}

		public Task WaitAsync() {
			lock(Waiters) {
				if(CurrentCount > 0) {
					CurrentCount--;
					return Task.FromResult(true);
				}

				var waiter = new TaskCompletionSource<bool>();
				Waiters.Enqueue(waiter);
				return waiter.Task;
			}
		}

		public void Release() {
			TaskCompletionSource<bool> toRelease = null;

			lock(Waiters) {
				if(Waiters.Count > 0)
					toRelease = Waiters.Dequeue();
				else
					CurrentCount++;
			}
			if(toRelease != null)
				toRelease.SetResult(true);
		}                 
	}
}

