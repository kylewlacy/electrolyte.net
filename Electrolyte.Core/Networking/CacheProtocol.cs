using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Electrolyte.Networking {
	public abstract class CacheProtocol : NetworkProtocol {
		public CacheProtocol NextCacheProtocol {
			get {
				List<CacheProtocol> cacheProtocols = Network.Protocols.OfType<CacheProtocol>().ToList();

				int nextIndex = cacheProtocols.LastIndexOf(this) + 1;
				if(nextIndex >= cacheProtocols.Count)
					return null;
				CacheProtocol next = cacheProtocols[nextIndex];
				if(!next.IsConnected)
					next.Connect();
				return next;
			}
		}

		public async virtual Task<Money> GetCachedBalanceAsync(Address address, ulong startHeight = 0) {
			return (NextCacheProtocol != null ? await NextCacheProtocol.GetCachedBalanceAsync(address, startHeight) : Money.Zero("BTC"));
		}
	}
}

