// WARNING
//
// This file has been generated automatically by Xamarin Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoMac.Foundation;
using System.CodeDom.Compiler;

namespace Electrolyte.OSX
{
	[Register ("UnlockSheetController")]
	partial class UnlockSheetController
	{
		[Outlet]
		MonoMac.AppKit.NSButton cancelButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSButton unlockButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSSecureTextField walletPassphraseField { get; set; }

		[Action ("Cancel:")]
		partial void Cancel (MonoMac.Foundation.NSObject sender);

		[Action ("Unlock:")]
		partial void Unlock (MonoMac.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (cancelButton != null) {
				cancelButton.Dispose ();
				cancelButton = null;
			}

			if (unlockButton != null) {
				unlockButton.Dispose ();
				unlockButton = null;
			}

			if (walletPassphraseField != null) {
				walletPassphraseField.Dispose ();
				walletPassphraseField = null;
			}
		}
	}

	[Register ("UnlockSheet")]
	partial class UnlockSheet
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
