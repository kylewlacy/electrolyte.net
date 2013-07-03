using System;

namespace Electrolyte {
	[Flags]
	public enum SigHash : byte {
		All          = 0x01,
		None         = 0x02,
		Single       = 0x03,
		AnyoneCanPay = 0x80
	}
}