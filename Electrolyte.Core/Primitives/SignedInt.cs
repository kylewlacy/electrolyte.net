using System;

namespace Electrolyte.Primitives {
	public class SignedInt {
		public Int32 Value;

		public SignedInt(byte[] bytes) {
			byte[] b = new byte[bytes.Length];
			bytes.CopyTo(b, 0);

			if(b.Length > 4)
				throw new ArgumentException(String.Format("Byte array (length {0}) is larger than 32-bit integer", b.Length));

			bool isNegative = ((byte)0x80 & b[b.Length - 1]) == 0x80;
			b[b.Length - 1] = (byte)((byte)0x7F & b[b.Length - 1]);

			Value = 0;
			for(int i = 0; i < b.Length; i++) {
				Value += b[i] << (8 * i);
			}

			if(isNegative)
				Value = -Value;
		}

		public SignedInt(Int32 value) {
			Value = value;
		}

		public byte[] ToByteArray() {
			bool isNegative = Value < 0;
			int abs = Math.Abs(Value);

			byte[] bytes = new byte[] { };
			switch(Length(Value)) {
			case 8:
				bytes = new byte[]{
					unchecked((byte)abs)
				};
				break;
			case 16:
				bytes = new byte[]{
					unchecked((byte)abs),
					unchecked((byte)(abs >> 8))
				};
				break;
			case 32:
				bytes = new byte[]{
					unchecked((byte)abs),
					unchecked((byte)(abs >> 8)),
					unchecked((byte)(abs >> 16)),
					unchecked((byte)(abs >> 24))
				};
				break;
			}

			if(isNegative)
				bytes[bytes.Length - 1] = (byte)((byte)0x80 | bytes[bytes.Length - 1]);

//			foreach(byte b in bytes) {
//				Console.Write("{0} ", b);
//			}
//			Console.WriteLine();

			return bytes;
		}

		static int Length(Int32 number) {
			if(SByte.MinValue < number && number <= SByte.MaxValue)
				return 8;
			else if(Int16.MinValue < number && number <= Int16.MaxValue)
				return 16;
			else
				return 32;
		}
	}
}

