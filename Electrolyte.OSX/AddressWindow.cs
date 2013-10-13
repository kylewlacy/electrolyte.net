using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace Electrolyte.OSX {
	public partial class AddressWindow : MonoMac.AppKit.NSWindow {

		#region Constructors

		// Called when created from unmanaged code
		public AddressWindow(IntPtr handle) : base(handle) {
			Initialize();
		}
		// Called when created directly from a XIB file
		[Export("initWithCoder:")]
		public AddressWindow(NSCoder coder) : base(coder) {
			Initialize();
		}
		// Shared initialization code
		void Initialize() {
		}

		#endregion

	}
}

