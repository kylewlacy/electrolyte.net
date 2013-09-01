using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace Electrolyte.OSX {
	public partial class MainWindow : NSWindow {
		[Export ("initWithCoder:")]
		public MainWindow(NSCoder coder) : base(coder) { Initialize(); }
		public MainWindow(IntPtr handle) : base(handle) { Initialize(); }

		void Initialize() { }
	}
}

