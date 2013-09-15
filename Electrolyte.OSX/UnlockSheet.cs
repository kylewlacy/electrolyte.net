using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace Electrolyte.OSX {
	public partial class UnlockSheet : NSWindow {
		[Export("initWithCoder:")]
		public UnlockSheet(NSCoder coder) : base (coder) { Initialize(); }
		public UnlockSheet(IntPtr handle) : base (handle) { Initialize(); }

		void Initialize() { }

	}
}

