using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace Electrolyte.OSX {
	public partial class ConfirmTransactionSheet : NSWindow {
		[Export("initWithCoder:")]
		public ConfirmTransactionSheet(NSCoder coder) : base(coder) { Initialize(); }
		public ConfirmTransactionSheet(IntPtr handle) : base(handle) { Initialize(); }

		void Initialize() { }
	}
}

