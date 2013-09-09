using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Electrolyte.Networking {
	public class BlockchainProtocol : NetworkProtocol {
		public Uri Address;

		WebClient Client;
		static readonly SemaphoreSlim clientLock = new SemaphoreSlim(1);

		public BlockchainProtocol(string address) {
			Client = new WebClient();
			Address = new Uri(address);
		}

		public override void Connect() {
			Client = new WebClient();
			base.Connect();
		}

		public override void Disconnect() {
			Client = null;
			base.Disconnect();
		}

		async Task<string> GetContentAsync(string path) {
			// TODO: Use `Client.DownloadStringTaskAsync` (once it becomes available?)
			await clientLock.WaitAsync();
			try {
				return await Task.Run(() => Client.DownloadString(new Uri(Address, path)));
			}
			finally {
				clientLock.Release();
			}
		}

		async Task<JToken> GetJsonAsync(string path) {
			return JToken.Parse(await GetContentAsync(path));
		}


		public override async Task<Money> GetAddressBalanceAsync(Address address, ulong startHeight = 0) {
			if(startHeight > 0)
				return await NextProtocol.GetAddressBalanceAsync(address, startHeight);
			JToken data = await GetJsonAsync(String.Format("/address/{0}?format=json", address));
			return new Money(data["final_balance"].Value<long>(), "BTC");
		}



		public async override Task<decimal> GetExchangeRateAsync(Money.CurrencyType c1, Money.CurrencyType c2) {
			if(c2 != Money.CurrencyType.FindByCode("BTC"))
				return await NextProtocol.GetExchangeRateAsync(c1, c2);

			return GetExchangeRate(c1, c2, await GetJsonAsync("/ticker"));
		}

		static decimal GetExchangeRate(Money.CurrencyType c1, Money.CurrencyType c2, JToken rates) {
			if(c2 != Money.CurrencyType.FindByCode("BTC"))
				throw new NotImplementedException();

			JToken rate = rates[c1.Code];
			if(rate == null)
				throw new Money.UnknownExchangeRateException();

			return rates[c1.Code]["last"].Value<decimal>();
		}
	}
}

