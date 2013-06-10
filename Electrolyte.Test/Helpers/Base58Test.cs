// Adapted From https://gist.github.com/CodesInChaos/3175971
using System;
using System.Collections.Generic;
using NUnit.Framework;
using Electrolyte.Helpers;

namespace Electrolyte.Test.Helpers {
	[TestFixture]
	public class Base58Test {
		// Test cases from https://github.com/bitcoin/bitcoin/blob/master/src/test/base58_tests.cpp
		Tuple<string, byte[]>[] testCases = new Tuple<string, byte[]>[] {
			Tuple.Create("",                                   new byte[]{}),
			Tuple.Create("1112",                               new byte[]{ 0x00, 0x00, 0x00, 0x01 }),
			Tuple.Create("2g",                                 new byte[]{ 0x61 }),
			Tuple.Create("a3gV",                               new byte[]{ 0x62, 0x62, 0x62 }),
			Tuple.Create("aPEr",                               new byte[]{ 0x63, 0x63, 0x63 }),
			Tuple.Create("2cFupjhnEsSn59qHXstmK2ffpLv2",       new byte[]{ 0x73, 0x69, 0x6D, 0x70, 0x6C, 0x79, 0x20, 0x61, 0x20, 0x6C, 0x6F, 0x6E, 0x67, 0x20, 0x73, 0x74, 0x72, 0x69, 0x6E, 0x67 }),
			Tuple.Create("1NS17iag9jJgTHD1VXjvLCEnZuQ3rJDE9L", new byte[]{ 0x00, 0xEB, 0x15, 0x23, 0x1D, 0xFC, 0xEB, 0x60, 0x92, 0x58, 0x86, 0xB6, 0x7D, 0x06, 0x52, 0x99, 0x92, 0x59, 0x15, 0xAE, 0xB1, 0x72, 0xC0, 0x66, 0x47 }),
			Tuple.Create("ABnLTmg",                            new byte[]{ 0x51, 0x6B, 0x6F, 0xCD, 0x0F }),
			Tuple.Create("3SEo3LWLoPntC",                      new byte[]{ 0xBF, 0x4F, 0x89, 0x00, 0x1E, 0x67, 0x02, 0x74, 0xDD }),
			Tuple.Create("3EFU7m",                             new byte[]{ 0x57, 0x2E, 0x47, 0x94 }),
			Tuple.Create("EJDM8drfXA6uyA",                     new byte[]{ 0xEC, 0xAC, 0x89, 0xCA, 0xD9, 0x39, 0x23, 0xc0, 0x23, 0x21 }),
			Tuple.Create("Rt5zm",                              new byte[]{ 0x10, 0xc8, 0x51, 0x1E }),
			Tuple.Create("1111111111",                         new byte[]{ 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 })
		};

		[Test]
		public void Encode() {
			foreach(var tuple in testCases) {
				var bytes = tuple.Item2;
				var expectedText = tuple.Item1;
				var actualText = Base58.Encode(bytes);
				Assert.AreEqual(expectedText, actualText);
			}
		}

		[Test]
		public void Decode() {
			foreach(var tuple in testCases) {
				var text = tuple.Item1;
				var expectedBytes = tuple.Item2;
				var actualBytes = Base58.Decode(text);
				Assert.AreEqual(BitConverter.ToString(expectedBytes), BitConverter.ToString(actualBytes));
			}
		}

		[Test]
		public void DecodeInvalidChar() {
			Assert.Throws<FormatException>(() => {
				Base58.Decode("ab0"); });
		}
		// Example address from https://en.bitcoin.it/wiki/Technical_background_of_version_1_Bitcoin_addresses
		byte[] addressBytes = new byte[] { 0x00, 0x01, 0x09, 0x66, 0x77, 0x60, 0x06, 0x95, 0x3D, 0x55, 0x67, 0x43, 0x9E, 0x5E, 0x39, 0xF8, 0x6A, 0x0D, 0x27, 0x3B, 0xEE };
		string addressText = "16UwLL9Risc3QfPqBUvKofHmBQ7wMtjvM";
		string brokenAddressText = "16UwLl9Risc3QfPqBUvKofHmBQ7wMtjvM";

		[Test]
		public void EncodeBitcoinAddress() {
			var actualText = Base58.EncodeWithChecksum(addressBytes);
			Assert.AreEqual(addressText, actualText);
		}

		[Test]
		public void DecodeBitcoinAddress() {
			var actualBytes = Base58.DecodeWithChecksum(addressText);
			Assert.AreEqual(BitConverter.ToString(addressBytes), BitConverter.ToString(actualBytes));
		}

		[Test]
		public void DecodeBrokenBitcoinAddress() {
			Assert.Throws<FormatException>(() => { Base58.DecodeWithChecksum(brokenAddressText); });
		}
	}
}

