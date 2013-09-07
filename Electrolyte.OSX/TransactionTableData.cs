using System;
using System.Linq;
using System.Drawing;
using System.Collections.Generic;
using MonoMac.AppKit;
using MonoMac.Foundation;
using Electrolyte;
using Electrolyte.Messages;

namespace Electrolyte.OSX {
	[Register("TransactionTableData")]
	public class TransactionTableData {
		[Register("TransactionDataSource")]
		public class TransactionDataSource : NSTableViewDataSource {
			public TransactionTableData Data;

			[Export("initWithCoder:")]
			public TransactionDataSource(NSCoder coder) : base(coder) { Initialize(); }
			public TransactionDataSource(IntPtr handle) : base(handle) { Initialize(); }
			public TransactionDataSource(TransactionTableData data) { Initialize(data); }
			public TransactionDataSource() { Initialize(); }
	
			void Initialize() { Initialize(new TransactionTableData()); }

			void Initialize(TransactionTableData data) {
				Data = data;
			}
	
			public override int GetRowCount(NSTableView tableView) {
				return Data.TransactionDeltas.Count;
			}
		}

		[Register("TransactionDelegate")]
		public class TransactionDelegate : NSTableViewDelegate {
			public TransactionTableData Data;

			[Export("initWithCoder:")]
			public TransactionDelegate(NSCoder coder) : base(coder) { Initialize(); }
			public TransactionDelegate(IntPtr handle) : base(handle) { Initialize(); }
			public TransactionDelegate(TransactionTableData data) { Initialize(data); }
			public TransactionDelegate() { Initialize(); }

			void Initialize() { Initialize(new TransactionTableData()); }

			void Initialize(TransactionTableData data) {
				Data = data;
			}

			public override NSView GetViewForItem(NSTableView tableView, NSTableColumn tableColumn, int row) {
				TransactionTableCellView tableCellView;

				switch(tableColumn.Identifier) {
				case "transactions":
					Transaction.Delta delta = Data.TransactionDeltas[row];

					tableCellView = (TransactionTableCellView)tableView.MakeView("transaction", this);
//					tableCellView.Value = String.Format("{0}{1}", delta.Value > Money.Zero("BTC") ? "+" : "", delta.Value);
//					tableCellView.Hash = delta.Transaction.Hash;
					tableCellView.Delta = delta;

					break;

				default:
					throw new NotImplementedException();
				}

				return tableCellView;
			}

			public override bool SelectionShouldChange(NSTableView tableView) {
				return false;
			}
		}

		public TransactionDataSource DataSource;
		public TransactionDelegate Delegate;
		public List<Transaction.Delta> TransactionDeltas;

		public TransactionTableData() {
			DataSource = new TransactionDataSource(this);
			Delegate = new TransactionDelegate(this);
			TransactionDeltas = new List<Transaction.Delta>();
		}
	}
}

