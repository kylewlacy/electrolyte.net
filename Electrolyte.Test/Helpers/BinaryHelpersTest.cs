// Adapted From https://gist.github.com/CodesInChaos/3175971
using System;
using System.Collections.Generic;
using NUnit.Framework;
using Electrolyte.Helpers;

namespace Electrolyte.Test.Helpers {
	[TestFixture]
	public class BinaryHelpersTest {
		// Test cases from https://github.com/bitcoin/bitcoin/blob/master/src/test/base58_tests.cpp
		Tuple<string, byte[]>[] testCases = new Tuple<string, byte[]>[] {
			Tuple.Create("",                                   new byte[]{}),
			Tuple.Create("1112",                               new byte[]{ 0x11, 0x12 }),
			Tuple.Create("2f",                                 new byte[]{ 0x2F }),
			Tuple.Create("a3bd",                               new byte[]{ 0xA3, 0xBD }),
			Tuple.Create("ABCD",                               new byte[]{ 0xAB, 0xCD }),
			Tuple.Create("0123456789ABCDEF",                   new byte[]{ 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF })
		};

		[Test]
		public void ByteArrayToHex() {
			foreach(var tuple in testCases) {
				var bytes = tuple.Item2;
				var expectedHex = tuple.Item1.ToLower();
				var actualHex = BinaryHelpers.ByteArrayToHex(bytes);
				Assert.AreEqual(expectedHex, actualHex);
			}
		}

		[Test]
		public void HexToByteArray() {
			foreach(var tuple in testCases) {
				var hex = tuple.Item1;
				var expectedBytes = tuple.Item2;
				var actualBytes = BinaryHelpers.HexToByteArray(hex);
				Assert.AreEqual(expectedBytes, actualBytes);
			}
		}
	}
}

