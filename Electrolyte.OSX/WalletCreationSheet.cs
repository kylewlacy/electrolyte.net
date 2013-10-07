using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace Electrolyte.OSX {
	public partial class WalletCreationSheet : NSWindow {
		[Export("initWithCoder:")]
		public WalletCreationSheet(NSCoder coder) : base(coder) { Initialize(); }
		public WalletCreationSheet(IntPtr handle) : base(handle) { Initialize(); }

		void Initialize() { }
	}
}

