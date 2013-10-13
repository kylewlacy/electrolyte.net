using System;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace Electrolyte.OSX {
	[Register("AddressTableData")]
	public class AddressTableData {
		public class AddressDataSource : NSTableViewDataSource {
			public AddressTableData Data;

			[Export("initWithCoder:")]
			public AddressDataSource(NSCoder coder) : base(coder) { Initialize(); }
			public AddressDataSource(IntPtr handle) : base(handle) { Initialize(); }
			public AddressDataSource(AddressTableData data) { Initialize(data); }
			public AddressDataSource() { Initialize(); }

			void Initialize() { Initialize(new AddressTableData()); }
			void Initialize(AddressTableData data) {
				Data = data;
			}

			public override int GetRowCount(NSTableView tableView) {
				return Data.Wallet.Addresses.Count;
			}

			public override NSObject GetObjectValue(NSTableView tableView, NSTableColumn tableColumn, int row) {
				string value = "";
				switch(tableColumn.Identifier) {
				case "address":
					value = Data.Wallet.Addresses.ToList()[row].ToString();
					break;
				}

				return new NSString(value);
			}
		}

		public class AddressDelegate : NSTableViewDelegate {
			public AddressTableData Data;

			[Export("initWithCoder:")]
			public AddressDelegate(NSCoder coder) : base(coder) { Initialize(); }
			public AddressDelegate(IntPtr handle) : base(handle) { Initialize(); }
			public AddressDelegate(AddressTableData data) { Initialize(data); }
			public AddressDelegate() { Initialize(); }

			void Initialize() { Initialize(new AddressTableData()); }
			void Initialize(AddressTableData data) {
				Data = data;
			}
		}

		public AddressDataSource DataSource;
		public Wallet Wallet;

		public AddressTableData(Wallet wallet = null) {
			Wallet = wallet;
			DataSource = new AddressDataSource(this);
		}
	}
}

