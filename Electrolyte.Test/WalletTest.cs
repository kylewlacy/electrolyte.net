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
			Wallet wallet = Wallet.CreateAsync(Encoding.ASCII.GetBytes("1234"), null).Result;
			wallet.ImportKeyAsync("5Kb8kLf9zgWQnogidDA76MzPL6TsZZY36hWXMssSzNydYXYB9KF").Wait();

			Assert.Contains(new ECKey(new byte[] { 0xE9, 0x87, 0x3D, 0x79, 0xC6, 0xD8, 0x7D, 0xC0, 0xFB, 0x6A, 0x57, 0x78, 0x63, 0x33, 0x89, 0xF4, 0x45, 0x32, 0x13, 0x30, 0x3D, 0xA6, 0x1F, 0x20, 0xBD, 0x67, 0xFC, 0x23, 0x3A, 0xA3, 0x32, 0x62 }), wallet.PrivateKeys.Values);
			Assert.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj"), wallet.PrivateKeys.Keys);
			Assert.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj"), wallet.Addresses.ToList());
		}

		[Test]
		public void WatchAddress() {
			Wallet wallet = Wallet.CreateAsync(Encoding.ASCII.GetBytes("1234"), null).Result;

			wallet.ImportWatchAddressAsync("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj").Wait();
			wallet.ImportWatchAddressAsync("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm").Wait();

			Assert.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj"), wallet.WatchAddresses.ToList());
			Assert.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm"), wallet.WatchAddresses.ToList());
		}

		[Test]
		public void PublicAddresses() {
			Wallet wallet = Wallet.CreateAsync(Encoding.ASCII.GetBytes("1234"), null).Result;
			
			wallet.ImportKeyAsync("5Kb8kLf9zgWQnogidDA76MzPL6TsZZY36hWXMssSzNydYXYB9KF", false).Wait();
			wallet.ImportKeyAsync("5KJD58353MLqgAdt6dqgwEGF4jDXcYN8bCpPsC5Qn2cqur6kZSw", true).Wait();

			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.PublicAddresses.ToList());
			Assert.IsFalse(wallet.PublicAddresses.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj")));
		}

		[Test]
		public void ReadWritePrivate() {
			Wallet writeWallet = Wallet.CreateAsync(Encoding.ASCII.GetBytes("1234"), null).Result;
			writeWallet.ImportKeyAsync("5Kb8kLf9zgWQnogidDA76MzPL6TsZZY36hWXMssSzNydYXYB9KF").Wait();

			byte[] output;
			using(MemoryStream stream = new MemoryStream()) {
				using(StreamWriter writer = new StreamWriter(stream))
					writeWallet.WritePrivateAsync(writer).Wait();
				output = stream.ToArray();
			}
			
			Wallet readWallet = Wallet.CreateAsync(Encoding.ASCII.GetBytes("1234"), null).Result;
			using(MemoryStream stream = new MemoryStream(output)) {
				using(StreamReader reader = new StreamReader(stream))
					readWallet.ReadPrivateAsync(reader).Wait();
			}

			Assert.AreEqual(writeWallet.Addresses, readWallet.Addresses);
			Assert.AreEqual(writeWallet.PrivateKeys, readWallet.PrivateKeys);
		}

		[Test]
		public void EncryptDecrypt() {
			Wallet encryptWallet = Wallet.CreateAsync(Encoding.ASCII.GetBytes("1234"), null).Result;
			encryptWallet.ImportKeyAsync("5Kb8kLf9zgWQnogidDA76MzPL6TsZZY36hWXMssSzNydYXYB9KF").Wait();
			encryptWallet.ImportKeyAsync("5KJD58353MLqgAdt6dqgwEGF4jDXcYN8bCpPsC5Qn2cqur6kZSw", true).Wait();
			encryptWallet.ImportWatchAddressAsync("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm").Wait();

			byte[] output;
			using(MemoryStream stream = new MemoryStream()) {
				using(StreamWriter writer = new StreamWriter(stream))
					encryptWallet.EncryptAsync(writer, "1234").Wait();
				output = stream.ToArray();
			}

			Wallet decryptWallet = Wallet.CreateAsync(Encoding.ASCII.GetBytes("1234"), null).Result;
			using(MemoryStream stream = new MemoryStream(output)) {
				using(StreamReader reader = new StreamReader(stream))
					decryptWallet.DecryptAsync(reader, "1234").Wait();
			}

			Assert.AreEqual(encryptWallet.PublicAddresses, decryptWallet.PublicAddresses);
			Assert.AreEqual(encryptWallet.WatchAddresses, decryptWallet.WatchAddresses);
		}

		[Test]
		public void InvalidPasscode() {
			Wallet encryptWallet = Wallet.CreateAsync(Encoding.ASCII.GetBytes("1234"), null).Result;
			encryptWallet.GenerateAddressAsync().Wait();

			byte[] output;
			using(MemoryStream stream = new MemoryStream()) {
				using(StreamWriter writer = new StreamWriter(stream))
					encryptWallet.EncryptAsync(writer, "1234").Wait();
				output = stream.ToArray();
			}

			using(MemoryStream stream = new MemoryStream(output)) {
				using(StreamReader reader = new StreamReader(stream))
					Assert.Throws<Wallet.InvalidPassphraseException>(() => {
						try { Wallet.CreateAsync(Encoding.ASCII.GetBytes("1234"), null).Result.DecryptAsync(reader, "2345").Wait(); } catch(AggregateException e) { throw e.InnerException; }
					});
			}
		}

		[Test]
		public void LockUnlock() {
			Wallet wallet = Wallet.CreateAsync(Encoding.ASCII.GetBytes("1234"), null).Result;
			wallet.ImportKeyAsync("5Kb8kLf9zgWQnogidDA76MzPL6TsZZY36hWXMssSzNydYXYB9KF").Wait();
			wallet.ImportKeyAsync("5KJD58353MLqgAdt6dqgwEGF4jDXcYN8bCpPsC5Qn2cqur6kZSw", true).Wait();
			wallet.ImportWatchAddressAsync("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm").Wait();

			Assert.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm"), wallet.WatchAddresses.ToList());



			wallet.LockAsync().Wait();
			Assert.IsFalse(wallet.WatchAddresses.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj")));
			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm"), wallet.WatchAddresses.ToList());

			wallet.UnlockAsync("1234").Wait();
			Assert.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm"), wallet.WatchAddresses.ToList());



			wallet.LockAsync().Wait();
			Assert.IsFalse(wallet.WatchAddresses.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj")));
			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm"), wallet.WatchAddresses.ToList());

			wallet.UnlockAsync("1234").Wait();
			Assert.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm"), wallet.WatchAddresses.ToList());



			wallet.LockAsync().Wait();
			Assert.IsFalse(wallet.WatchAddresses.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj")));
			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm"), wallet.WatchAddresses.ToList());

			Assert.Throws<Wallet.InvalidPassphraseException>(() => {
				try { wallet.UnlockAsync("2345").Wait(); } catch(AggregateException e) { throw e.InnerException; }
			});
			Assert.IsFalse(wallet.WatchAddresses.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj")));
			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm"), wallet.WatchAddresses.ToList());



			wallet.UnlockAsync("1234").Wait();
			Assert.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm"), wallet.WatchAddresses.ToList());

			Assert.Throws<Wallet.OperationException>(() => {
				try { wallet.UnlockAsync("1234").Wait(); } catch (AggregateException e) { throw e.InnerException; }
			});
			Assert.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm"), wallet.WatchAddresses.ToList());



			wallet.LockAsync().Wait();
			Assert.Throws<Wallet.OperationException>(() => {
				try { wallet.LockAsync().Wait(); } catch(AggregateException e) { throw e.InnerException; }
			});
			wallet.UnlockAsync("1234").Wait();

			wallet.LockAsync().Wait();
			wallet.UnlockAsync("1234").Wait();
		}

		[Test]
		public void UnlockTimeout() {
			Wallet wallet = Wallet.CreateAsync(Encoding.ASCII.GetBytes("1234"), null).Result;
			wallet.ImportKeyAsync("5Kb8kLf9zgWQnogidDA76MzPL6TsZZY36hWXMssSzNydYXYB9KF").Wait();

			wallet.LockAsync().Wait();
			Assert.IsTrue(wallet.IsLocked);

			wallet.UnlockAsync("1234", 500).Wait();
			Assert.IsFalse(wallet.IsLocked);

			Thread.Sleep(1000);

			Assert.IsTrue(wallet.IsLocked);

		}

		[Test]
		public void ImportPrivateKey() {
			Wallet wallet = Wallet.CreateAsync(Encoding.ASCII.GetBytes("1234"), null).Result;
			wallet.ImportKeyAsync("5Kb8kLf9zgWQnogidDA76MzPL6TsZZY36hWXMssSzNydYXYB9KF").Wait();

			ECKey key = wallet.PrivateKeys.Values.ToArray()[0];
			Assert.AreEqual("5Kb8kLf9zgWQnogidDA76MzPL6TsZZY36hWXMssSzNydYXYB9KF", key.ToWalletImportFormat());

			Assert.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj"), wallet.PrivateKeys.Keys);
		}

		[Test]
		public void AccessLimitation() {
			Wallet wallet = Wallet.CreateAsync(Encoding.ASCII.GetBytes("1234"), null).Result;
			wallet.ImportKeyAsync("5Kb8kLf9zgWQnogidDA76MzPL6TsZZY36hWXMssSzNydYXYB9KF", false).Wait();
			wallet.ImportKeyAsync("5KJD58353MLqgAdt6dqgwEGF4jDXcYN8bCpPsC5Qn2cqur6kZSw", true).Wait();
			wallet.ImportWatchAddressAsync("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm").Wait();

			Assert.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.PublicAddresses.ToList());
			Assert.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm"), wallet.WatchAddresses.ToList());

			wallet.LockAsync().Wait();
			
			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.PublicAddresses.ToList());
			Assert.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm"), wallet.WatchAddresses.ToList());
			Assert.IsFalse(wallet.Addresses.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj")));

			wallet.UnlockAsync("1234").Wait();

			Assert.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.PublicAddresses.ToList());
			Assert.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm"), wallet.WatchAddresses.ToList());
		}

		[Test]
		public void AddressManagement() {
			Wallet wallet = Wallet.CreateAsync(Encoding.ASCII.GetBytes("1234"), null).Result;
			wallet.ImportKeyAsync("5Kb8kLf9zgWQnogidDA76MzPL6TsZZY36hWXMssSzNydYXYB9KF", false).Wait();
			wallet.ImportKeyAsync("5KJD58353MLqgAdt6dqgwEGF4jDXcYN8bCpPsC5Qn2cqur6kZSw", true).Wait();
			wallet.ImportWatchAddressAsync("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm").Wait();

			Assert.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm"), wallet.WatchAddresses.ToList());

			wallet.LockAsync().Wait();

			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm"), wallet.WatchAddresses.ToList());
			Assert.IsFalse(wallet.Addresses.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj")));

			wallet.UnlockAsync("1234").Wait();

			Assert.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm"), wallet.WatchAddresses.ToList());

			wallet.ShowAddressAsync("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj").Wait();
			wallet.LockAsync().Wait();

			Assert.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm"), wallet.WatchAddresses.ToList());

			wallet.UnlockAsync("1234").Wait();
			wallet.HideAddressAsync("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q").Wait();

			Assert.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm"), wallet.WatchAddresses.ToList());

			wallet.LockAsync().Wait();

			Assert.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm"), wallet.WatchAddresses.ToList());
			Assert.IsFalse(wallet.Addresses.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q")));

			wallet.UnlockAsync("1234").Wait();
			wallet.RemoveWatchAddressAsync("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm").Wait();

			Assert.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj"), wallet.Addresses.ToList());
			Assert.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q"), wallet.Addresses.ToList());
			Assert.IsFalse(wallet.Addresses.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm")));

			wallet.LockAsync().Wait();

			Assert.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj"), wallet.Addresses.ToList());
			Assert.IsFalse(wallet.Addresses.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q")));
			Assert.IsFalse(wallet.Addresses.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm")));

			wallet.UnlockAsync("1234").Wait();
			wallet.RemoveAddressAsync("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q").Wait();

			Assert.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj"), wallet.Addresses.ToList());
			Assert.IsFalse(wallet.Addresses.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q")));
			Assert.IsFalse(wallet.Addresses.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm")));

			wallet.LockAsync().Wait();

			Assert.Contains(new Address("1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj"), wallet.Addresses.ToList());
			Assert.IsFalse(wallet.Addresses.Contains(new Address("1CyuTPXMVqdHpDD7WTVcEvRFe4GmTHZC1Q")));
			Assert.IsFalse(wallet.Addresses.Contains(new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm")));
		}
	}
}

