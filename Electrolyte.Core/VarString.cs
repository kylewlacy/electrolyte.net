using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Electrolyte {
	public class VarString {
		public string Value;

		public VarString(string value) {
			Value = value;
		}

		public VarString(char[] value) {
			Value = new String(value);
		}

		public static VarString FromBinaryReader(BinaryReader reader) {
			UInt64 length;
			return FromBinaryReader(reader, out length);
		}

		public static VarString FromBinaryReader(BinaryReader reader, out UInt64 length) {
			length = VarInt.FromBinaryReader(reader).Value;

			return new VarString(reader.ReadChars((int)length));
		}
	}
}

