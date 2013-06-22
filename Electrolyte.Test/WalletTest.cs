using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace Electrolyte.Test {
	[TestFixture]
	public class WalletTest {
		[Test]
		public void ReadWrite() {
			Wallet writeWallet = new Wallet();
			writeWallet.GenerateKey();

			byte[] output;
			using(MemoryStream stream = new MemoryStream()) {
				using(StreamWriter writer = new StreamWriter(stream))
					writeWallet.Write(writer);
				output = stream.ToArray();
			}
			
			Wallet readWallet;
			using(MemoryStream stream = new MemoryStream(output)) {
				using(StreamReader reader = new StreamReader(stream))
					readWallet = Wallet.Read(reader);
			}

			Assert.AreEqual(readWallet.PrivateKeys.Keys.ToArray()[0], writeWallet.PrivateKeys.Keys.ToArray()[0]);
			Assert.AreEqual(Marshal.PtrToStringBSTR(Marshal.SecureStringToBSTR(readWallet.PrivateKeys.Values.ToArray()[0])), Marshal.PtrToStringBSTR(Marshal.SecureStringToBSTR(writeWallet.PrivateKeys.Values.ToArray()[0])));
		}

		[Test]
		public void EncryptDecrypt() {
			Wallet encryptWallet = new Wallet();
			encryptWallet.GenerateKey();

			byte[] output;
			using(MemoryStream stream = new MemoryStream()) {
				using(BinaryWriter writer = new BinaryWriter(stream))
					encryptWallet.Encrypt(writer, "1234");
				output = stream.ToArray();
			}

			Wallet decryptWallet;
			using(MemoryStream stream = new MemoryStream(output)) {
				using(BinaryReader reader = new BinaryReader(stream))
					decryptWallet = Wallet.Decrypt(reader, "1234");
			}

			Assert.AreEqual(encryptWallet.PrivateKeys.Keys.ToArray()[0], decryptWallet.PrivateKeys.Keys.ToArray()[0]);
			Assert.AreEqual(Marshal.PtrToStringBSTR(Marshal.SecureStringToBSTR(encryptWallet.PrivateKeys.Values.ToArray()[0])), Marshal.PtrToStringBSTR(Marshal.SecureStringToBSTR(decryptWallet.PrivateKeys.Values.ToArray()[0])));
		}

		[Test]
		public void InvalidPasscode() {
			Wallet encryptWallet = new Wallet();
			encryptWallet.GenerateKey();

			byte[] output;
			using(MemoryStream stream = new MemoryStream()) {
				using(BinaryWriter writer = new BinaryWriter(stream))
					encryptWallet.Encrypt(writer, "1234");
				output = stream.ToArray();
			}

			using(MemoryStream stream = new MemoryStream(output)) {
				using(BinaryReader reader = new BinaryReader(stream))
					Assert.Throws<CryptographicException>(() => Wallet.Decrypt(reader, "2345"));
			}
		}
	}
}

