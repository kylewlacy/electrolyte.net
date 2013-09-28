using System;
using System.IO;
using System.Threading.Tasks;
using Electrolyte;
using Electrolyte.Helpers;
using Electrolyte.Messages;
using Electrolyte.Portable.IO;
using Electrolyte.Portable.Cryptography;
using FileInfo = Electrolyte.Portable.IO.FileInfo;
using FileStream = Electrolyte.Portable.IO.FileStream;
using FileMode = Electrolyte.Portable.IO.FileMode;

namespace Electrolyte.Networking {
	public class FileCacheProtocol : CacheProtocol {
		public override void Connect() {
		}

		public override async Task<Transaction> GetTransactionAsync(Transaction.Info info) {
			PathInfo txCache = DefaultCachePath.SubPath("transactions");
			FileInfo cache = FileInfo.Create(txCache, String.Format("{0}.{1}", info.Hash, "tx"));

			Transaction tx;
			if(cache.Exists) {
				tx = new Transaction();
				tx.ReadPayload(new BinaryReader(FileStream.Create(cache, FileMode.Open)));
				tx.Height = info.Height;
			}
			else {
				tx = await base.GetTransactionAsync(info);
				tx.WritePayload(new BinaryWriter(FileStream.Create(cache, FileMode.Create)));
			}
			return tx;
		}

		public override async Task<Money> GetAddressBalanceAsync(Address address, ulong startHeight = 0) {
			PathInfo balanceCache = DefaultCachePath.SubPath("balances");
			FileInfo cache = FileInfo.Create(balanceCache, String.Format("{0}.{1}", GetHashedAddress(address), "bal"));

			Money balance = await base.GetAddressBalanceAsync(address, startHeight);
			using(var writer = new StreamWriter(FileStream.Create(cache, FileMode.Create))) {
				await writer.WriteLineAsync(balance.Cents.ToString());
			}
			return balance;
		}

		public override async Task<Money> GetCachedBalanceAsync(Address address, ulong startHeight = 0) {
			PathInfo balanceCache = DefaultCachePath.SubPath("balances");
			FileInfo cache = FileInfo.Create(balanceCache, String.Format("{0}.{1}", GetHashedAddress(address), "bal"));


			if(cache.Exists) {
				using(var stream = FileStream.Create(cache, FileMode.Open)) {
					using(var reader = new StreamReader(stream)) {
						return new Money(Int64.Parse(await reader.ReadLineAsync()), "BTC");
					}
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



		public static PathInfo DefaultCachePath {
			get { return PathInfo.DefaultStoragePath.SubPath("cache"); }
		}
	}
}

