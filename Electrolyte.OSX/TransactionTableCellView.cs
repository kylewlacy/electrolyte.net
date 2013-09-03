using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;
using MonoMac.CoreGraphics;

namespace Electrolyte.OSX {
	[Register ("TransactionTableCellView")]
	public class TransactionTableCellView : NSTableCellView {
		public static NSColor PositiveColor {
			get { return NSColor.FromDeviceRgba(0.40f, 0.76f, 0.08f, 1.00f);  }
		}

		public static NSColor NegativeColor {
			get { return NSColor.FromDeviceRgba(0.77f, 0.00f, 0.07f, 1.00f); }
		}

		public static NSColor NeutralColor {
			get { return NSColor.FromDeviceRgba(0.54f, 0.54f, 0.54f, 1.00f); }
		}

		public NSColor BackgroundColor;

		[Outlet]
		MonoMac.AppKit.NSTextField hashLabel { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField timeLabel { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField valueLabel { get; set; }

		public string Value {
			get { return valueLabel.StringValue; }
			set { valueLabel.StringValue = value; }
		}

		public string Hash {
			get { return hashLabel.StringValue; }
			set { hashLabel.StringValue = value; }
		}

		public string Time {
			get { return timeLabel.StringValue; }
			set { timeLabel.StringValue = value; }
		}

		[Export ("initWithCoder:")]
		public TransactionTableCellView(NSCoder coder) : base (coder) { Initialize(); }
		public TransactionTableCellView(IntPtr handle) : base (handle) { Initialize(); }

		void Initialize() { }

		public override void DrawRect(System.Drawing.RectangleF dirtyRect) {
			BackgroundColor.SetFill();
			NSBezierPath.FillRect(dirtyRect);
		}
	}
}

