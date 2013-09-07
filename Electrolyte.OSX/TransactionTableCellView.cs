using System;
using MonoMac.Foundation;
using MonoMac.AppKit;
using Electrolyte.Messages;

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

		Transaction.Delta _delta;
		public Transaction.Delta Delta {
			get { return _delta; }
			set {
				_delta = value;
				Hash = _delta.Transaction.Hash;
				// Time = DateTime.Now - _delta.Transaction.Time;
				Value = _delta.ToString();

				if(_delta.Value > Money.Zero("BTC"))
					BackgroundColor = PositiveColor;
				else if(_delta.Value < Money.Zero("BTC"))
					BackgroundColor = NegativeColor;
				else
					BackgroundColor = NeutralColor;
			}
		}

		public NSColor BackgroundColor { get; private set; }

		[Outlet]
		NSTextField hashLabel { get; set; }

		[Outlet]
		NSTextField timeLabel { get; set; }

		[Outlet]
		NSTextField valueLabel { get; set; }

		public string Hash {
			get { return hashLabel.StringValue; }
			private set { hashLabel.StringValue = value; }
		}

		public string Time {
			get { return timeLabel.StringValue; }
			private set { timeLabel.StringValue = value; }
		}

		public string Value {
			get { return valueLabel.StringValue; }
			private set { valueLabel.StringValue = value; }
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

