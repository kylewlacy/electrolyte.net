using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using NUnit.Framework;
using Electrolyte;

namespace Electrolyte.Test {
	[TestFixture]
	public class VarStringTest {
		[Test]
		public void TestLength() {
			byte[] string1 = {
				0x05,							// Length is 5
				0x68, 0x65, 0x6C, 0x6C, 0x6F	// "hello"
			};

			byte[] string2Length = {
				0xFD,							// Length type is UInt16
				0x01, 0xEF						// Lenght is 495
			};

			// From http://slipsum.com/
			string loremIpsum = "The path of the righteous man is beset on all sides by the " +
				"iniquities of the selfish and the tyranny of evil men. Blessed is he who, " +
				"in the name of charity and good will, shepherds the weak through the valley " +
				"of darkness, for he is truly his brother's keeper and the finder of lost " +
				"children. And I will strike down upon thee with great vengeance and furious " +
				"anger those who would attempt to poison and destroy My brothers. And you will " +
				"know My name is the Lord when I lay My vengeance upon thee.";

			List<byte> string2 = new List<byte>(string2Length);
			string2.AddRange(Encoding.UTF8.GetBytes(loremIpsum.ToCharArray()));

			using(BinaryReader reader = new BinaryReader(new MemoryStream(string1))) {
				Assert.AreEqual(VarString.FromBinaryReader(reader).Value, "hello");
			}

			using(BinaryReader reader = new BinaryReader(new MemoryStream(string2.ToArray()))) {
				Assert.AreEqual(VarString.FromBinaryReader(reader).Value, loremIpsum);
			}
		}
	}
}

