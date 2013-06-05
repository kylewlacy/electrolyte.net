using System;
using System.IO;
using NUnit.Framework;
using Electrolyte;

namespace Electrolyte.Test {
	[TestFixture]
	public class VarIntTest {
		[Test]
		public void Test8Bit() {
			byte[] bytes = {0xFC};

			using(BinaryReader reader = new BinaryReader(new MemoryStream(bytes))) {
				int length;
				UInt64 value = VarInt.FromBinaryReader(reader, out length).Value;

				Assert.AreEqual(length, 8);
				Assert.AreEqual(value, 252);
			}
		}

		[Test]
		public void Test16Bit() {
			byte[] numericBytes = BitConverter.GetBytes((ushort)65432);
			byte[] bytes = {0xFD,
				numericBytes[0], numericBytes[1]
			};

			using(BinaryReader reader = new BinaryReader(new MemoryStream(bytes))) {
				int length;
				UInt64 value = VarInt.FromBinaryReader(reader, out length).Value;

				Assert.AreEqual(length, 16);
				Assert.AreEqual(value, 65432);
			}
		}

		[Test]
		public void Test32Bit() {
			byte[] numericBytes = BitConverter.GetBytes(4109876543U);
			byte[] bytes = {0xFE,
				numericBytes[0], numericBytes[1], numericBytes[2], numericBytes[3]
			};

			using(MemoryStream stream = new MemoryStream(bytes))
			using(BinaryReader reader = new BinaryReader(stream)) {
				int length;
				UInt64 value = VarInt.FromBinaryReader(reader, out length).Value;

				Assert.AreEqual(length, 32);
				Assert.AreEqual(value, 4109876543);
			}
		}

		[Test]
		public void Test64Bit() {
			byte[] numericBytes = BitConverter.GetBytes(17654321098765432109UL);
			byte[] bytes = {0xFF,
				numericBytes[0], numericBytes[1], numericBytes[2], numericBytes[3], numericBytes[4], numericBytes[5], numericBytes[6], numericBytes[7]
			};

			using(BinaryReader reader = new BinaryReader(new MemoryStream(bytes))) {
				int length;
				UInt64 value = VarInt.FromBinaryReader(reader, out length).Value;

				Assert.AreEqual(length, 64);
				Assert.AreEqual(value, 17654321098765432109);
			}
		}
	}
}

