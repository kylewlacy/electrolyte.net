using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Electrolyte.Networking {
	public abstract class CacheProtocol : NetworkProtocol {
		public CacheProtocol NextCacheProtocol {
			get {
				int nextIndex = Network.CacheProtocols.LastIndexOf(this) + 1;
				if(nextIndex >= Network.CacheProtocols.Count)
					return null;
				CacheProtocol next = Network.CacheProtocols[nextIndex];
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

