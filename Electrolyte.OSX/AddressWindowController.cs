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

		enum AddSegment {
			AddPrompt = 1,
			QuickAdd = 2
		}

		public Wallet Wallet {
			get {
				return AddressTableData.Wallet;
			}
			set {
				Console.WriteLine("Setup did lock");
				Wallet.DidLock += LockToggled;
				Wallet.DidUnlock += LockToggled;
				AddressTableData.Wallet = value;
			}
		}

		void Initialize(Wallet wallet = null) {
			wallet.DidLock += LockToggled;
			wallet.DidUnlock += LockToggled;
			AddressTableData = new AddressTableData(wallet);
		}

		public new AddressWindow Window {
			get { return (AddressWindow)base.Window; }
		}

		public override void WindowDidLoad() {
			AddressTableData.TableView = addressTable;
			addressTable.DataSource = AddressTableData.DataSource;
			addressTable.Delegate = AddressTableData.Delegate;

			LockToggled();
		}

		void LockToggled() {
			InvokeOnMainThread(() => LockToggled(null, null));
		}

		void LockToggled(object sender, EventArgs e) {
			addControl.Enabled = !Wallet.IsLocked;
		}

		async partial void AddNewAddress(NSSegmentedControl sender) {
			int tag = sender.Cell.GetTag(sender.SelectedSegment);
			if(!Wallet.IsLocked && Enum.IsDefined(typeof(AddSegment), tag)) {
				switch((AddSegment)tag) {
				case AddSegment.AddPrompt:
					break;
				case AddSegment.QuickAdd:
					var newAddress = await Wallet.GenerateAddressAsync();
					AddressTableData.Reload();
					break;
				}
			}
		}
	}
}

