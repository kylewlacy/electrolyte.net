using System;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;
using Electrolyte.Portable;
using System.Threading.Tasks;

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
				return Data.Addresses.Count;
			}

			public override NSObject GetObjectValue(NSTableView tableView, NSTableColumn tableColumn, int row) {
				try {
					string value = null;
					var details = Data.Addresses[row];

					switch(tableColumn.Identifier) {
					case "address":
						value = details.Address.ToString();
						break;
					case "label":
						value = details.Label;
						break;
					}

					return new NSString(value ?? "");
				}
				catch {
					return new NSString();
				}
			}

			public override async void SetObjectValue(NSTableView tableView, NSObject theObject, NSTableColumn tableColumn, int row) {
				var details = Data.Addresses[row];
				var stringObject = theObject as NSString;
				var value = stringObject != null ? stringObject.ToString() : "";

				switch(tableColumn.Identifier) {
				case "label":
					Data.Addresses[row].Label = value;
					await Data.Wallet.SetLabelAsync(details.Address.ToString(), value, true);

					break;
				}
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

			public override bool ShouldEditTableColumn(NSTableView tableView, NSTableColumn tableColumn, int row) {
				switch(tableColumn.Identifier) {
				case "label":
					return !Data.Wallet.IsLocked;
				default:
					return false;
				}
			}
		}

		public AddressDataSource DataSource;
		public AddressDelegate Delegate;

		Wallet _wallet;
		public Wallet Wallet {
			get {
				return _wallet;
			}
			set {
				_wallet = value;
				Reload();
				_wallet.DidLock += (sender, e) => Reload();
				_wallet.DidUnlock += (sender, e) => Reload();
			}
		}

		public AddressCollection Addresses { get; set; }

		public NSTableView TableView { get; set; }

		public AddressTableData(Wallet wallet = null, NSTableView tableView = null) {
			Wallet = wallet;
			TableView = tableView;
			DataSource = new AddressDataSource(this);
			Delegate = new AddressDelegate(this);
		}

		public void Reload() {
			Addresses = new AddressCollection(Wallet.Addresses.ToList());
			if(TableView != null)
				TableView.ReloadData();
		}
	}
}

