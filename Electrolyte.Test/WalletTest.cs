using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using Electrolyte.Primitives;
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
					writeWallet.WritePrivate(writer);
				output = stream.ToArray();
			}
			
			Wallet readWallet = new Wallet();
			using(MemoryStream stream = new MemoryStream(output)) {
				using(StreamReader reader = new StreamReader(stream))
					readWallet.ReadPrivate(reader);
			}

			Assert.AreEqual(readWallet.PrivateKeys.Keys.ToArray()[0], writeWallet.PrivateKeys.Keys.ToArray()[0]);
			Assert.AreEqual(readWallet.PrivateKeys.Values.ToArray()[0], writeWallet.PrivateKeys.Values.ToArray()[0]);
		}

		[Test]
		public void EncryptDecrypt() {
			Wallet encryptWallet = new Wallet();
			encryptWallet.GenerateKey();

			byte[] output;
			using(MemoryStream stream = new MemoryStream()) {
				using(StreamWriter writer = new StreamWriter(stream))
					encryptWallet.Encrypt(writer, "1234");
				output = stream.ToArray();
			}

			Wallet decryptWallet = new Wallet();
			using(MemoryStream stream = new MemoryStream(output)) {
				using(StreamReader reader = new StreamReader(stream))
					decryptWallet.Decrypt(reader, "1234");
			}

			Assert.AreEqual(encryptWallet.PrivateKeys.Keys.ToArray()[0], decryptWallet.PrivateKeys.Keys.ToArray()[0]);
			Assert.AreEqual(encryptWallet.PrivateKeys.Values.ToArray()[0], decryptWallet.PrivateKeys.Values.ToArray()[0]);
		}

		[Test]
		public void InvalidPasscode() {
			Wallet encryptWallet = new Wallet();
			encryptWallet.GenerateKey();

			byte[] output;
			using(MemoryStream stream = new MemoryStream()) {
				using(StreamWriter writer = new StreamWriter(stream))
					encryptWallet.Encrypt(writer, "1234");
				output = stream.ToArray();
			}

			using(MemoryStream stream = new MemoryStream(output)) {
				using(StreamReader reader = new StreamReader(stream))
					Assert.Throws<CryptographicException>(() => new Wallet().Decrypt(reader, "2345"));
			}
		}

		[Test]
		public void ImportPrivateKey() {
			Wallet wallet = new Wallet();
			wallet.ImportKey("5Kb8kLf9zgWQnogidDA76MzPL6TsZZY36hWXMssSzNydYXYB9KF");

			ECKey key = new ECKey(wallet.PrivateKeys.Values.ToArray()[0]);
			Assert.AreEqual("5Kb8kLf9zgWQnogidDA76MzPL6TsZZY36hWXMssSzNydYXYB9KF", key.ToWalletImportFormat());

			Assert.Contains("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj", wallet.PrivateKeys.Keys);
		}
	}
}

