using System;
using System.IO;
using System.Threading.Tasks;
using Electrolyte;
using Electrolyte.Messages;

namespace Electrolyte.Networking {
	public class FileCacheProtocol : NetworkProtocol {
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



		public static string DefaultCachePath {
			get { return Path.Combine(Wallet.DefaultStoragePath, "cache"); }
		}
	}
}

