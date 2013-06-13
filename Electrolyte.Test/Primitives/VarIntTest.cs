using System;
using System.IO;
using NUnit.Framework;
using Electrolyte.Primitives;

namespace Electrolyte.Test.Primitives {
	[TestFixture]
	public class VarIntTest {
		Tuple<byte[], ulong>[] Numbers = new Tuple<byte[], ulong>[] {
			Tuple.Create(new byte[] { 0xFC }, 252UL),
			Tuple.Create(new byte[] { 0xFD, 0x98, 0xFF }, 65432UL),
			Tuple.Create(new byte[] { 0xFE, 0x3F, 0xBD, 0xF7, 0xF4 }, 4109876543UL),
			Tuple.Create(new byte[] { 0xFF, 0x2D, 0xF5, 0x98, 0xB2, 0x80, 0xBF, 0x00, 0xF5 }, 17654321098765432109UL)
		};

		[Test]
		public void Read() {
			foreach(var number in Numbers) {
				using(BinaryReader reader = new BinaryReader(new MemoryStream(number.Item1))) {
					Assert.AreEqual(VarInt.Read(reader).Value, number.Item2);
				}
			}
		}

		[Test]
		public void Write() {
			foreach(var number in Numbers) {
				using(MemoryStream stream = new MemoryStream())
				using(BinaryWriter writer = new BinaryWriter(stream)) {
					VarInt v = new VarInt(number.Item2);
					v.Write(writer);

					Assert.AreEqual(stream.ToArray(), number.Item1);
				}
			}
		}
	}
}

