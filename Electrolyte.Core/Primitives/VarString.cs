using System;
using System.IO;

namespace Electrolyte.Primitives {
	public class VarString {
		public string Value;

		public VarString(string value) {
			Value = value;
		}

		public VarString(char[] value) {
			Value = new String(value);
		}

		public static VarString Read(BinaryReader reader) {
			UInt64 length;
			return Read(reader, out length);
		}

		public static VarString Read(BinaryReader reader, out UInt64 length) {
			length = VarInt.Read(reader).Value;
			return new VarString(reader.ReadChars((int)length));
		}
	}
}

