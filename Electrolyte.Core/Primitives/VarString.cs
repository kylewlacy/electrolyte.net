using System;
using System.IO;
using System.Text;

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

		public void Write(BinaryWriter writer) {
			var v = new VarInt(Value.Length);
			v.Write(writer);
			writer.Write(Encoding.UTF8.GetBytes(Value));
		}
	}
}

