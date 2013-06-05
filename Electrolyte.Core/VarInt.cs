using System;
using System.Collections.Generic;
using System.IO;

namespace Electrolyte {
	public class VarInt {
		UInt64 Value;

		public VarInt(UInt64 value) {
			Value = value;
		}

		public VarInt(Int64 value) {
			Value = (UInt64)value;
		}
		// http://code.google.com/p/bitcoinj/source/browse/core/src/main/java/com/google/bitcoin/core/VarInt.java
		public VarInt FromBinaryReader(BinaryReader reader) {
			byte first = reader.ReadByte();

			switch(intLength(first)) {
			case 8:
			case 16:
			case 32:
			case 64:
			default:
				throw new Exception();
			}
		}

		static int intLength(byte first) {
			if(first <= 252)
				return 8;
			else if(first <= 253)
				return 16;
			else if(first <= 254)
				return 32;
			else
				return 64;

		}
	}
}

