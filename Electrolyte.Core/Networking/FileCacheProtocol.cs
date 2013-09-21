using System;
using System.IO;
using System.Threading.Tasks;
using Electrolyte;
using Electrolyte.Helpers;
using Electrolyte.Messages;
using Electrolyte.Cryptography;

namespace Electrolyte.Networking {
	public class FileCacheProtocol : CacheProtocol {
		public override void Connect() {
			if(!Directory.Exists(DefaultCachePath))
				Directory.CreateDirectory(DefaultCachePath);
		}

		public override async Task<Transaction> GetTransactionAsync(Transaction.Info info) {
			string txCache = Path.Combine(DefaultCachePath, "transactions");
			if(!Directory.Exists(txCache))
				Directory.CreateDirectory(txCache);
			string cache = Path.Combine(txCache, String.Format("{0}.{1}", info.Hash, "tx"));

			Transaction tx;
			if(File.Exists(cache)) {
				tx = new Transaction();
				tx.ReadPayload(new BinaryReader(new FileStream(cache, FileMode.Open)));
				tx.Height = info.Height;
			}
			else {
				tx = await base.GetTransactionAsync(info);
				tx.WritePayload(new BinaryWriter(new FileStream(cache, FileMode.Create)));
			}
			return tx;
		}

		public override async Task<Money> GetAddressBalanceAsync(Address address, ulong startHeight = 0) {
			string balanceCache = Path.Combine(DefaultCachePath, "balances");
			if(!Directory.Exists(balanceCache))
				Directory.CreateDirectory(balanceCache);
			string cache = Path.Combine(balanceCache, String.Format("{0}.{1}", GetHashedAddress(address), "bal"));

			Money balance = await base.GetAddressBalanceAsync(address, startHeight);
			using(var writer = new StreamWriter(new FileStream(cache, FileMode.Create))) {
				await writer.WriteLineAsync(balance.Cents.ToString());
			}
			return balance;
		}

		public override async Task<Money> GetCachedBalanceAsync(Address address, ulong startHeight = 0) {
			string balanceCache = Path.Combine(DefaultCachePath, "balances");
			if(!Directory.Exists(balanceCache))
				Directory.CreateDirectory(balanceCache);
			string cache = Path.Combine(balanceCache, String.Format("{0}.{1}", GetHashedAddress(address), "bal"));


			if(File.Exists(cache)) {
				using(var reader = new StreamReader(cache)) {
					return new Money(Int64.Parse(await reader.ReadLineAsync()), "BTC");
				}
			}

			return await base.GetCachedBalanceAsync(address, startHeight);
		}

		static string GetHashedAddress(Address address) {
			byte[] currentHash = Base58.DecodeWithChecksum(address.ID);
			for(int i = 0; i < 1024; i++) {
				currentHash = SHA256.Hash(currentHash);
			}

			return Base58.Encode(currentHash);
		}



		public static string DefaultCachePath {
			get { return Path.Combine(Wallet.DefaultStoragePath, "cache"); }
		}
	}
}

