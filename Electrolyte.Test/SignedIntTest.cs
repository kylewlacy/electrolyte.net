using System;
using Electrolyte.Portable;
using NUnit.Framework;

namespace Electrolyte.Test {
	[TestFixture]
	public class SignedIntTest {
		Tuple<byte[], int>[] Ints = new Tuple<byte[], int>[] {
			Tuple.Create(new byte[] { 0x00 }, 0),
			Tuple.Create(new byte[] { 0x01 }, 1),
			Tuple.Create(new byte[] { 0x81 }, -1),
			Tuple.Create(new byte[] { 0x7F }, 127),
			Tuple.Create(new byte[] { 0xFF }, -127),
			Tuple.Create(new byte[] { 0x00, 0x7F }, 32512),
			Tuple.Create(new byte[] { 0x00, 0xFF }, -32512),
			Tuple.Create(new byte[] { 0x12, 0x34, 0x56, 0x78 }, 2018915346),
			Tuple.Create(new byte[] { 0x12, 0x34, 0x56, 0xF8 }, -2018915346),
			Tuple.Create(new byte[] { 0xFF, 0xFF, 0xFF, 0x7F }, 2147483647),
			Tuple.Create(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, -2147483647)
		};

		[Test]
		public void FromByteArray() {
			foreach(var integer in Ints) {
				Assert.AreEqual(integer.Item2, new SignedInt(integer.Item1).Value);
			}
		}

		[Test]
		public void ToByteArray() {
			foreach(var integer in Ints) {
				Assert.AreEqual(integer.Item1, new SignedInt(integer.Item2).ToByteArray());
			}
		}

		[Test]
		public void Transcriptiom() {
			for(int i = Int16.MinValue - 100; i < Int16.MaxValue + 100; i++) { // Arbitrary range, but the Int32 values are too slow
				Assert.AreEqual(i, new SignedInt(new SignedInt(i).ToByteArray()).Value);
			}
		}

		[Test]
		public void NegativeZero() {
			SignedInt positive0 = new SignedInt(new byte[] { 0x00 });
			SignedInt negative0 = new SignedInt(new byte[] { 0x80 });

			Assert.AreEqual(0, positive0.Value);
			Assert.AreEqual(0, negative0.Value);
			Assert.AreEqual(negative0.ToByteArray(), positive0.ToByteArray());
		}
	}
}

