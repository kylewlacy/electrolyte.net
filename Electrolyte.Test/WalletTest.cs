using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using Electrolyte.Primitives;
using NUnit.Framework;

namespace Electrolyte.Test {
	[TestFixture]
	public class WalletTest {
		[Test]
		public void ImportKeys() {
			Wallet wallet = new Wallet(Encoding.ASCII.GetBytes("1234"), null);
			wallet.ImportKey("5Kb8kLf9zgWQnogidDA76MzPL6TsZZY36hWXMssSzNydYXYB9KF");

			Assert.Contains(new ECKey(new byte[] { 0xE9, 0x87, 0x3D, 0x79, 0xC6, 0xD8, 0x7D, 0xC0, 0xFB, 0x6A, 0x57, 0x78, 0x63, 0x33, 0x89, 0xF4, 0x45, 0x32, 0x13, 0x30, 0x3D, 0xA6, 0x1F, 0x20, 0xBD, 0x67, 0xFC, 0x23, 0x3A, 0xA3, 0x32, 0x62 }), wallet.PrivateKeys.Values);
			Assert.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj"), wallet.PrivateKeys.Keys);
			Assert.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj"), wallet.Addresses.ToList());
		}

		[Test]
		public void WatchAddress() {
			Wallet wallet = new Wallet(Encoding.ASCII.GetBytes("1234"), null);

			wallet.ImportWatchAddress("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj");
			wallet.ImportWatchAddress("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm");

			Assert.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj"), wallet.WatchAddresses.ToList());
			Assert.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm"), wallet.WatchAddresses.ToList());
		}

		[Test]
		public void PublicAddresses() {
			Wallet wallet = new Wallet(Encoding.ASCII.GetBytes("1234"), null);
			
			wallet.ImportKey("5Kb8kLf9zgWQnogidDA76MzPL6TsZZY36hWXMssSzNydYXYB9KF");
			wallet.ImportKey("5KJD58353MLqgAdt6dqgwEGF4jDXcYN8bCpPsC5Qn2cqur6kZSw", true);

			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.PublicAddresses.ToList());
			Assert.IsFalse(wallet.PublicAddresses.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj")));
		}

		[Test]
		public void ReadWritePrivate() {
			Wallet writeWallet = new Wallet(Encoding.ASCII.GetBytes("1234"), null);
			writeWallet.ImportKey("5Kb8kLf9zgWQnogidDA76MzPL6TsZZY36hWXMssSzNydYXYB9KF");

			byte[] output;
			using(MemoryStream stream = new MemoryStream()) {
				using(StreamWriter writer = new StreamWriter(stream))
					writeWallet.WritePrivate(writer);
				output = stream.ToArray();
			}
			
			Wallet readWallet = new Wallet(Encoding.ASCII.GetBytes("1234"), null);
			using(MemoryStream stream = new MemoryStream(output)) {
				using(StreamReader reader = new StreamReader(stream))
					readWallet.ReadPrivate(reader);
			}

			Assert.AreEqual(writeWallet.Addresses, readWallet.Addresses);
			Assert.AreEqual(writeWallet.PrivateKeys, readWallet.PrivateKeys);
		}

		[Test]
		public void EncryptDecrypt() {
			Wallet encryptWallet = new Wallet(Encoding.ASCII.GetBytes("1234"), null);
			encryptWallet.ImportKey("5Kb8kLf9zgWQnogidDA76MzPL6TsZZY36hWXMssSzNydYXYB9KF");
			encryptWallet.ImportKey("5KJD58353MLqgAdt6dqgwEGF4jDXcYN8bCpPsC5Qn2cqur6kZSw", true);
			encryptWallet.ImportWatchAddress("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm");

			byte[] output;
			using(MemoryStream stream = new MemoryStream()) {
				using(StreamWriter writer = new StreamWriter(stream))
					encryptWallet.Encrypt(writer, "1234");
				output = stream.ToArray();
			}

			Wallet decryptWallet = new Wallet(Encoding.ASCII.GetBytes("1234"), null);
			using(MemoryStream stream = new MemoryStream(output)) {
				using(StreamReader reader = new StreamReader(stream))
					decryptWallet.Decrypt(reader, "1234");
			}

			Assert.AreEqual(encryptWallet.PublicAddresses, decryptWallet.PublicAddresses);
			Assert.AreEqual(encryptWallet.WatchAddresses, decryptWallet.WatchAddresses);
		}

		[Test]
		public void InvalidPasscode() {
			Wallet encryptWallet = new Wallet(Encoding.ASCII.GetBytes("1234"), null);
			encryptWallet.GenerateAddress();

			byte[] output;
			using(MemoryStream stream = new MemoryStream()) {
				using(StreamWriter writer = new StreamWriter(stream))
					encryptWallet.Encrypt(writer, "1234");
				output = stream.ToArray();
			}

			using(MemoryStream stream = new MemoryStream(output)) {
				using(StreamReader reader = new StreamReader(stream))
					Assert.Throws<CryptographicException>(() => new Wallet(Encoding.ASCII.GetBytes("1234"), null).Decrypt(reader, "2345"));
			}
		}

		[Test]
		public void LockUnlock() {
			Wallet wallet = new Wallet(Encoding.ASCII.GetBytes("1234"), null);
			wallet.ImportKey("5Kb8kLf9zgWQnogidDA76MzPL6TsZZY36hWXMssSzNydYXYB9KF");
			wallet.ImportKey("5KJD58353MLqgAdt6dqgwEGF4jDXcYN8bCpPsC5Qn2cqur6kZSw", true);
			wallet.ImportWatchAddress("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm");

			Assert.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm"), wallet.WatchAddresses.ToList());



			wallet.Lock();
			Assert.IsFalse(wallet.WatchAddresses.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj")));
			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm"), wallet.WatchAddresses.ToList());

			wallet.Unlock("1234");
			Assert.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm"), wallet.WatchAddresses.ToList());



			wallet.Lock();
			Assert.IsFalse(wallet.WatchAddresses.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj")));
			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm"), wallet.WatchAddresses.ToList());

			wallet.Unlock("1234");
			Assert.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm"), wallet.WatchAddresses.ToList());



			wallet.Lock();
			Assert.IsFalse(wallet.WatchAddresses.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj")));
			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm"), wallet.WatchAddresses.ToList());

			Assert.Throws<CryptographicException>(() => wallet.Unlock("2345"));
			Assert.IsFalse(wallet.WatchAddresses.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj")));
			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm"), wallet.WatchAddresses.ToList());



			wallet.Unlock("1234");			
			Assert.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm"), wallet.WatchAddresses.ToList());

			Assert.Throws<Wallet.OperationException>(() => wallet.Unlock("1234"));
			Assert.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm"), wallet.WatchAddresses.ToList());



			wallet.Lock();
			Assert.Throws<Wallet.OperationException>(wallet.Lock);
			wallet.Unlock("1234");

			wallet.Lock();
			wallet.Unlock("1234");
		}

		[Test]
		public void UnlockTimeout() {
			Wallet wallet = new Wallet(Encoding.ASCII.GetBytes("1234"), null);
			wallet.ImportKey("5Kb8kLf9zgWQnogidDA76MzPL6TsZZY36hWXMssSzNydYXYB9KF");

			wallet.Lock();
			Assert.IsTrue(wallet.IsLocked);

			wallet.Unlock("1234", 500);
			Assert.IsFalse(wallet.IsLocked);

			Thread.Sleep(1000);

			Assert.IsTrue(wallet.IsLocked);

		}

		[Test]
		public void ImportPrivateKey() {
			Wallet wallet = new Wallet(Encoding.ASCII.GetBytes("1234"), null);
			wallet.ImportKey("5Kb8kLf9zgWQnogidDA76MzPL6TsZZY36hWXMssSzNydYXYB9KF");

			ECKey key = wallet.PrivateKeys.Values.ToArray()[0];
			Assert.AreEqual("5Kb8kLf9zgWQnogidDA76MzPL6TsZZY36hWXMssSzNydYXYB9KF", key.ToWalletImportFormat());

			Assert.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj"), wallet.PrivateKeys.Keys);
		}

		[Test]
		public void AccessLimitation() {
			Wallet wallet = new Wallet(Encoding.ASCII.GetBytes("1234"), null);
			wallet.ImportKey("5Kb8kLf9zgWQnogidDA76MzPL6TsZZY36hWXMssSzNydYXYB9KF");
			wallet.ImportKey("5KJD58353MLqgAdt6dqgwEGF4jDXcYN8bCpPsC5Qn2cqur6kZSw", true);
			wallet.ImportWatchAddress("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm");

			Assert.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.PublicAddresses.ToList());
			Assert.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm"), wallet.WatchAddresses.ToList());

			wallet.Lock();
			
			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.PublicAddresses.ToList());
			Assert.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm"), wallet.WatchAddresses.ToList());
			Assert.IsFalse(wallet.Addresses.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj")));

			wallet.Unlock("1234");

			Assert.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.PublicAddresses.ToList());
			Assert.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm"), wallet.WatchAddresses.ToList());
		}

		[Test]
		public void AddressManagement() {
			Wallet wallet = new Wallet(Encoding.ASCII.GetBytes("1234"), null);
			wallet.ImportKey("5Kb8kLf9zgWQnogidDA76MzPL6TsZZY36hWXMssSzNydYXYB9KF");
			wallet.ImportKey("5KJD58353MLqgAdt6dqgwEGF4jDXcYN8bCpPsC5Qn2cqur6kZSw", true);
			wallet.ImportWatchAddress("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm");

			Assert.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm"), wallet.WatchAddresses.ToList());

			wallet.Lock();

			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm"), wallet.WatchAddresses.ToList());
			Assert.IsFalse(wallet.Addresses.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj")));

			wallet.Unlock("1234");

			Assert.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm"), wallet.WatchAddresses.ToList());

			wallet.ShowAddress("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj");
			wallet.Lock();

			Assert.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm"), wallet.WatchAddresses.ToList());

			wallet.Unlock("1234");
			wallet.HideAddress("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q");

			Assert.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm"), wallet.WatchAddresses.ToList());

			wallet.Lock();

			Assert.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm"), wallet.WatchAddresses.ToList());
			Assert.IsFalse(wallet.Addresses.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q")));

			wallet.Unlock("1234");
			wallet.RemoveWatchAddress("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm");

			Assert.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.Addresses.ToList());
			Assert.IsFalse(wallet.Addresses.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm")));

			wallet.Lock();

			Assert.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj"), wallet.Addresses.ToList());
			Assert.IsFalse(wallet.Addresses.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q")));
			Assert.IsFalse(wallet.Addresses.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm")));

			wallet.Unlock("1234");
			wallet.RemoveAddress("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q");

			Assert.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj"), wallet.Addresses.ToList());
			Assert.IsFalse(wallet.Addresses.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q")));
			Assert.IsFalse(wallet.Addresses.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm")));

			wallet.Lock();

			Assert.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj"), wallet.Addresses.ToList());
			Assert.IsFalse(wallet.Addresses.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q")));
			Assert.IsFalse(wallet.Addresses.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm")));
		}
	}
}

