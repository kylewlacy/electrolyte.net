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
	[Register ("WalletCreationSheetController")]
	partial class WalletCreationSheetController
	{
		[Outlet]
		MonoMac.AppKit.NSButton createButton { get; set; }

		[Outlet]
		MonoMac.AppKit.NSSecureTextField passphraseField { get; set; }

		[Action ("Cancel:")]
		partial void Cancel (MonoMac.Foundation.NSObject sender);

		[Action ("Create:")]
		partial void Create (MonoMac.Foundation.NSObject sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (passphraseField != null) {
				passphraseField.Dispose ();
				passphraseField = null;
			}

			if (createButton != null) {
				createButton.Dispose ();
				createButton = null;
			}
		}
	}

	[Register ("WalletCreationSheet")]
	partial class WalletCreationSheet
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
