using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using Electrolyte.Primitives;
using Electrolyte.Helpers;

namespace Electrolyte.Test.Primitives {
	[TestFixture]
	public class VarStringTest {
		static string string1 = "hello";

		static byte[] string1payload = {
			0x05,							// Length is 5
			0x68, 0x65, 0x6C, 0x6C, 0x6F	// "hello"
		};

		// From http://slipsum.com/
		static string string2 = "The path of the righteous man is beset on all sides by the " +
			"iniquities of the selfish and the tyranny of evil men. Blessed is he who, " +
				"in the name of charity and good will, shepherds the weak through the valley " +
				"of darkness, for he is truly his brother's keeper and the finder of lost " +
				"children. And I will strike down upon thee with great vengeance and furious " +
				"anger those who would attempt to poison and destroy My brothers. And you will " +
				"know My name is the Lord when I lay My vengeance upon thee.";

		byte[] string2payload = ArrayHelpers.ConcatArrays(
			new byte[] { 0xFD, 0x01, 0xEF },
			Encoding.UTF8.GetBytes(string2)
		);

		[Test]
		public void TestRead() {
			using(BinaryReader reader = new BinaryReader(new MemoryStream(string1payload))) {
				Assert.AreEqual(VarString.FromBinaryReader(reader).Value, "hello");
			}

			using(BinaryReader reader = new BinaryReader(new MemoryStream(string2payload))) {
				Assert.AreEqual(VarString.FromBinaryReader(reader).Value, string2);
			}
		}
	}
}

