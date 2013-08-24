using System;
using System.Linq;

namespace Electrolyte.Helpers {
	public enum Endian { Big, Little }

	public static class BinaryHelpers {
		public static Endian SystemEndianness {
			get {
				if(BitConverter.IsLittleEndian)
					return Endian.Little;
				else
					return Endian.Big;
			}
		}

		public static byte[] SetEndianness(this byte[] bytes, Endian endianness) {
			if(BinaryHelpers.SystemEndianness == endianness)
				return bytes;

			return bytes.Reverse().ToArray();
		}

		public static char[] SetEndianness(this char[] chars, Endian endianness) {
			if(BinaryHelpers.SystemEndianness == endianness)
				return chars;

			return chars.Reverse().ToArray();
		}



		public static string ByteArrayToHex(byte[] bytes) {
			return BitConverter.ToString(bytes).Replace("-", "").ToLower();
		}

		public static byte[] HexToByteArray(string hex) {
			// TODO: Refactor
			return Enumerable.Range(0, hex.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(hex.Substring(x, 2), 16)).ToArray();
		}
	}
}

