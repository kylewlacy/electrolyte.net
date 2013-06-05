using System;
using System.Collections.Generic;
using System.IO;

namespace Electrolyte {
	public class VarInt {
		public UInt64 Value;

		public VarInt(UInt64 value) {
			Value = value;
		}

		public VarInt(Int64 value) {
			Value = (UInt64)value;
		}

		public static VarInt FromBinaryReader(BinaryReader reader) {
			int length;
			return FromBinaryReader(reader, out length);
		}

		// http://code.google.com/p/bitcoinj/source/browse/core/src/main/java/com/google/bitcoin/core/VarInt.java
		public static VarInt FromBinaryReader(BinaryReader reader, out int length) {
			byte first = reader.ReadByte();
			length = intLength(first);

			// TODO: Check for invalid number of bits in reader
			switch(length) {
			case 8:
				return new VarInt(first);
			case 16:
				return new VarInt(BitConverter.ToUInt16(reader.ReadBytes(2), 0));
			case 32:
				return new VarInt(BitConverter.ToUInt32(reader.ReadBytes(4), 0));
			case 64:
				return new VarInt(BitConverter.ToUInt64(reader.ReadBytes(8), 0));
			}

			throw new Exception("Invalid int length");
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

