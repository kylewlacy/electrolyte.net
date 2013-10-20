using System;
using System.Collections.Generic;
using System.Linq;
using MonoMac.Foundation;
using MonoMac.AppKit;

namespace Electrolyte.OSX {
	public partial class AddressWindowController : NSWindowController {
		[Export("initWithCoder:")]
		public AddressWindowController(NSCoder coder) : base(coder) { Initialize(); }
		public AddressWindowController(IntPtr handle) : base(handle) { Initialize(); }
		public AddressWindowController(Wallet wallet = null) : base("AddressWindow") { Initialize(wallet); }

		AddressTableData _addressTableData;
		public AddressTableData AddressTableData {
			get { return (_addressTableData = _addressTableData ?? new AddressTableData()); }
			protected set { _addressTableData = value; }
		}

		public Wallet Wallet {
			get { return AddressTableData.Wallet; }
			set { AddressTableData.Wallet = value; }
		}

		void Initialize(Wallet wallet = null) {
			AddressTableData = new AddressTableData(wallet);
		}

		public new AddressWindow Window {
			get { return (AddressWindow)base.Window; }
		}

		public override void WindowDidLoad() {
			AddressTableData.TableView = addressTable;
			addressTable.DataSource = AddressTableData.DataSource;
			addressTable.Delegate = AddressTableData.Delegate;
		}
	}
}

