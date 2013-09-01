using System;
using System.Collections.Generic;
using MonoMac.AppKit;
using MonoMac.Foundation;
using Electrolyte;
using Electrolyte.Messages;

namespace Electrolyte.OSX {
	[Register("TransactionDataSource")]
	public class TransactionDataSource : NSTableViewDataSource {
		public List<KeyValuePair<Transaction, Money>> TransactionDeltas;
		
		[Export ("initWithCoder:")]
		public TransactionDataSource(NSCoder coder) : base(coder) { Initialize(); }
		public TransactionDataSource(IntPtr handle) : base(handle) { Initialize(); }
		public TransactionDataSource() { Initialize(); }

		void Initialize() {
			TransactionDeltas = new List<KeyValuePair<Transaction, Money>>();
		}

		public override int GetRowCount(NSTableView tableView) {
			return TransactionDeltas.Count;
		}

		public override NSObject GetObjectValue(NSTableView tableView, NSTableColumn tableColumn, int row) {
			KeyValuePair<Transaction, Money> delta = TransactionDeltas[row];

			string value;
			switch(tableColumn.Identifier) {
			case "value":
				value = String.Format("{0}{1}", delta.Value > Money.Zero("BTC") ? "+" : "", delta.Value);
				break;
			case "hash":
				value = delta.Key.Hash;
				break;
			default:
				value = "";
				break;
			}

			return new NSString(value);
		}
	}
}

