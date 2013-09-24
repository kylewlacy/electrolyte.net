using System;
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

		public static VarInt Read(BinaryReader reader) {
			int length;
			return Read(reader, out length);
		}

		// http://code.google.com/p/bitcoinj/source/browse/core/src/main/java/com/google/bitcoin/core/VarInt.java
		public static VarInt Read(BinaryReader reader, out int length) {
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

		public void Write(BinaryWriter writer) {
			if(Value <= 252) {
				writer.Write((byte)Value);
			}
			else if(Value <= UInt16.MaxValue) {
				writer.Write((byte)253);
				writer.Write((UInt16)Value);
			}
			else if(Value <= UInt32.MaxValue) {
				writer.Write((byte)254);
				writer.Write((UInt32)Value);
			}
			else {
				writer.Write((byte)255);
				writer.Write(Value);
			}
		}

		static int intLength(byte first) {
			if(first <= 252)
				return 8;
			if(first <= 253)
				return 16;
			if(first <= 254)
				return 32;
			return 64;

		}
	}
}

