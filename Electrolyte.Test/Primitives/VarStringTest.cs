using System;
using System.IO;
using System.Text;
using NUnit.Framework;
using Electrolyte.Primitives;
using Electrolyte.Helpers;

namespace Electrolyte.Test.Primitives {
	[TestFixture]
	public class VarStringTest {
		// From http://slipsum.com/
		static string loremIpsum = "The path of the righteous man is beset on all sides by the " +
			"iniquities of the selfish and the tyranny of evil men. Blessed is he who, " +
				"in the name of charity and good will, shepherds the weak through the valley " +
				"of darkness, for he is truly his brother's keeper and the finder of lost " +
				"children. And I will strike down upon thee with great vengeance and furious " +
				"anger those who would attempt to poison and destroy My brothers. And you will " +
				"know My name is the Lord when I lay My vengeance upon thee.";

		Tuple<string, byte[]>[] Strings = new Tuple<string, byte[]>[] {
			Tuple.Create("hello", new byte[] { 0x05 }),
			Tuple.Create(loremIpsum, new byte[] { 0xFD, 0x01, 0xEF })
		};

		[Test]
		public void Read() {
			foreach(var str in Strings) {
				byte[] full = ArrayHelpers.ConcatArrays(str.Item2, Encoding.ASCII.GetBytes(str.Item1));

				using(BinaryReader reader = new BinaryReader(new MemoryStream(full))) {
					VarString v = VarString.Read(reader);
					Assert.AreEqual(v.Value, str.Item1);
				}
			}
		}
	}
}

