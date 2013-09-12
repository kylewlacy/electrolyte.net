using System;
using System.IO;
using Electrolyte.Helpers;

namespace Electrolyte.Extensions {
	public static class BinaryReaderExtensions {
		public static char[] ReadChars(this BinaryReader reader, int count, Endian endianness) {
			return reader.ReadChars(count).SetEndianness(endianness);
		}

		public static byte[] ReadBytes(this BinaryReader reader, int count, Endian endianness) {
			return reader.ReadBytes(count).SetEndianness(endianness);
		}

		public static short ReadInt16(this BinaryReader reader, Endian endianness) {
			return BitConverter.ToInt16(reader.ReadBytes(sizeof(short), endianness), 0);
		}

		public static int ReadInt32(this BinaryReader reader, Endian endianness) {
			return BitConverter.ToInt32(reader.ReadBytes(sizeof(int), endianness), 0);
		}

		public static long ReadInt64(this BinaryReader reader, Endian endianness) {
			return BitConverter.ToInt64(reader.ReadBytes(sizeof(long), endianness), 0);
		}

		public static ushort ReadUInt16(this BinaryReader reader, Endian endianness) {
			return BitConverter.ToUInt16(reader.ReadBytes(sizeof(ushort), endianness), 0);
		}

		public static uint ReadUInt32(this BinaryReader reader, Endian endianness) {
			return BitConverter.ToUInt32(reader.ReadBytes(sizeof(uint), endianness), 0);
		}

		public static ulong ReadUInt64(this BinaryReader reader, Endian endianness) {
			return BitConverter.ToUInt64(reader.ReadBytes(sizeof(ulong), endianness), 0);
		}

		public static float ReadSingle(this BinaryReader reader, Endian endianness) {
			return BitConverter.ToSingle(reader.ReadBytes(sizeof(float), endianness), 0);
		}

		public static double ReadDouble(this BinaryReader reader, Endian endianness) {
			return BitConverter.ToDouble(reader.ReadBytes(sizeof(double), endianness), 0);
		}
	}

	public static class BinaryWriterExtensions {
		public static void Write(this BinaryWriter writer, byte[] bytes, Endian endianness) {
			writer.Write(bytes.SetEndianness(endianness));
		}

		public static void Write(this BinaryWriter writer, char[] chars, Endian endianness) {
			writer.Write(chars.SetEndianness(endianness));
		}

		public static void Write(this BinaryWriter writer, Int16 n, Endian endianness) {
			writer.Write(BitConverter.GetBytes(n).SetEndianness(endianness));
		}

		public static void Write(this BinaryWriter writer, Int32 n, Endian endianness) {
			writer.Write(BitConverter.GetBytes(n).SetEndianness(endianness));
		}

		public static void Write(this BinaryWriter writer, Int64 n, Endian endianness) {
			writer.Write(BitConverter.GetBytes(n).SetEndianness(endianness));
		}

		public static void Write(this BinaryWriter writer, UInt16 n, Endian endianness) {
			writer.Write(BitConverter.GetBytes(n).SetEndianness(endianness));
		}

		public static void Write(this BinaryWriter writer, UInt32 n, Endian endianness) {
			writer.Write(BitConverter.GetBytes(n).SetEndianness(endianness));
		}

		public static void Write(this BinaryWriter writer, UInt64 n, Endian endianness) {
			writer.Write(BitConverter.GetBytes(n).SetEndianness(endianness));
		}

		public static void Write(this BinaryWriter writer, Single n, Endian endianness) {
			writer.Write(BitConverter.GetBytes(n).SetEndianness(endianness));
		}

		public static void Write(this BinaryWriter writer, Double n, Endian endianness) {
			writer.Write(BitConverter.GetBytes(n).SetEndianness(endianness));
		}
	}
}

