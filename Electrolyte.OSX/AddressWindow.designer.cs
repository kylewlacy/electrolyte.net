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
	[Register ("AddressWindowController")]
	partial class AddressWindowController
	{
		[Outlet]
		MonoMac.AppKit.NSSegmentedControl addControl { get; set; }

		[Outlet]
		MonoMac.AppKit.NSTableView addressTable { get; set; }

		[Action ("AddNewAddress:")]
		partial void AddNewAddress (MonoMac.AppKit.NSSegmentedControl sender);
		
		void ReleaseDesignerOutlets ()
		{
			if (addControl != null) {
				addControl.Dispose ();
				addControl = null;
			}

			if (addressTable != null) {
				addressTable.Dispose ();
				addressTable = null;
			}
		}
	}

	[Register ("AddressWindow")]
	partial class AddressWindow
	{
		
		void ReleaseDesignerOutlets ()
		{
		}
	}
}
