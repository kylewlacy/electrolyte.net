// From https://gist.github.com/CodesInChaos/3175971
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Electrolyte.Helpers {
	// Implements https://en.bitcoin.it/wiki/Base58Check_encoding
	public static class Base58 {
		public const int ChecksumSizeInBytes = 4;

		public static byte[] AddChecksum(byte[] data) {
			byte[] checkSum = GetChecksum(data);
			byte[] dataWithCheckSum = ArrayHelpers.ConcatArrays(data, checkSum);
			return dataWithCheckSum;
		}
		//Returns null if the checksum is invalid
		public static byte[] VerifyAndRemoveChecksum(byte[] data) {
			byte[] result = ArrayHelpers.SubArray(data, 0, data.Length - ChecksumSizeInBytes);
			byte[] givenCheckSum = ArrayHelpers.SubArray(data, data.Length - ChecksumSizeInBytes);
			byte[] correctCheckSum = GetChecksum(result);
			if(givenCheckSum.SequenceEqual(correctCheckSum))
				return result;
			else
				return null;
		}

		private const string Digits = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";

		public static string Encode(byte[] data) {
			// Decode byte[] to BigInteger
			BigInteger intData = 0;
			for(int i = 0; i < data.Length; i++) {
				intData = intData * 256 + data[i];
			}

			// Encode BigInteger to Base58 string
			string result = "";
			while(intData > 0) {
				int remainder = (int)(intData % 58);
				intData /= 58;
				result = Digits[remainder] + result;
			}

			// Append `1` for each leading 0 byte
			for(int i = 0; i < data.Length && data[i] == 0; i++) {
				result = '1' + result;
			}
			return result;
		}

		public static string EncodeWithChecksum(byte[] data) {
			return Encode(AddChecksum(data));
		}

		public static byte[] Decode(string s) {
			// Decode Base58 string to BigInteger 
			BigInteger intData = 0;
			for(int i = 0; i < s.Length; i++) {
				int digit = Digits.IndexOf(s[i]); //Slow
				if(digit < 0)
					throw new FormatException(string.Format("Invalid Base58 character `{0}` at position {1}", s[i], i));
				intData = intData * 58 + digit;
			}

			// Encode BigInteger to byte[]
			// Leading zero bytes get encoded as leading `1` characters
			int leadingZeroCount = s.TakeWhile(c => c == '1').Count();
			var leadingZeros = Enumerable.Repeat((byte)0, leadingZeroCount);
			var bytesWithoutLeadingZeros =
				intData.ToByteArray()
					.Reverse()// to big endian
					.SkipWhile(b => b == 0);//strip sign byte
			var result = leadingZeros.Concat(bytesWithoutLeadingZeros).ToArray();
			return result;
		}
		// Throws `FormatException` if s is not a valid Base58 string, or the checksum is invalid
		public static byte[] DecodeWithChecksum(string s) {
			var dataWithCheckSum = Decode(s);
			var dataWithoutCheckSum = VerifyAndRemoveChecksum(dataWithCheckSum);
			if(dataWithoutCheckSum == null)
				throw new FormatException("Base58 checksum is invalid");
			return dataWithoutCheckSum;
		}

		private static byte[] GetChecksum(byte[] data) {
			SHA256 sha256 = new SHA256Managed();
			byte[] hash1 = sha256.ComputeHash(data);
			byte[] hash2 = sha256.ComputeHash(hash1);

			var result = new byte[ChecksumSizeInBytes];
			Buffer.BlockCopy(hash2, 0, result, 0, result.Length);

			return result;
		}
	}
}