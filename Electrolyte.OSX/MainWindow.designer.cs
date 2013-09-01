// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;

namespace Electrolyte.OSX
{
	[Register ("MainWindowController")]
	partial class MainWindowController
	{
		[Outlet]
		MonoMac.AppKit.NSTextField balanceLabel { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton cancelUnlockButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton lockUnlockToggleButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField recipientField { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton sendButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTableView transactionTable { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton unlockButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSWindow unlockSheet { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTextField valueField { get; set; }

		[Outlet]
		MonoMac.AppKit.NSSecureTextField walletPassphraseField { get; set; }

		[Action ("CloseUnlockSheet:")]
		partial void CloseUnlockSheet (MonoMac.Foundation.NSObject sender);

		[Action ("SendTransaction:")]
		partial void SendTransaction (MonoMac.Foundation.NSObject sender);

		[Action ("ToggleLockUnlock:")]
		partial void ToggleLockUnlock (MonoMac.Foundation.NSObject sender);

		[Action ("UnlockWallet:")]
		partial void UnlockWallet (MonoMac.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (sendButton != null) {
				sendButton.Dispose ();
				sendButton = null;
			}

			if (lockUnlockToggleButton != null) {
				lockUnlockToggleButton.Dispose ();
				lockUnlockToggleButton = null;
			}

			if (walletPassphraseField != null) {
				walletPassphraseField.Dispose ();
				walletPassphraseField = null;
			}

			if (unlockSheet != null) {
				unlockSheet.Dispose ();
				unlockSheet = null;
			}

			if (unlockButton != null) {
				unlockButton.Dispose ();
				unlockButton = null;
			}

			if (cancelUnlockButton != null) {
				cancelUnlockButton.Dispose ();
				cancelUnlockButton = null;
			}

			if (balanceLabel != null) {
				balanceLabel.Dispose ();
				balanceLabel = null;
			}

			if (recipientField != null) {
				recipientField.Dispose ();
				recipientField = null;
			}

			if (transactionTable != null) {
				transactionTable.Dispose ();
				transactionTable = null;
			}

			if (valueField != null) {
				valueField.Dispose ();
				valueField = null;
			}
		}
	}

	[Register ("MainWindow")]
	partial class MainWindow
	{
		[Outlet]
		MonoMac.AppKit.NSTextField balanceLabel { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton sendButton { get; set; }

		[Outlet]
		Electrolyte.OSX.TransactionDataSource txData { get; set; }

		[Action ("SendClick:")]
		partial void SendClick (MonoMac.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (balanceLabel != null) {
				balanceLabel.Dispose ();
				balanceLabel = null;
			}

			if (sendButton != null) {
				sendButton.Dispose ();
				sendButton = null;
			}

			if (txData != null) {
				txData.Dispose ();
				txData = null;
			}
		}
	}
}
