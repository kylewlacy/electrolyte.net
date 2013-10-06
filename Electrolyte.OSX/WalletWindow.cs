using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace Electrolyte.OSX {
	public partial class WalletWindow : NSWindow {
		[Export ("initWithCoder:")]
		public WalletWindow(NSCoder coder) : base(coder) { Initialize(); }
		public WalletWindow(IntPtr handle) : base(handle) { Initialize(); }

		void Initialize() { }
	}
}

