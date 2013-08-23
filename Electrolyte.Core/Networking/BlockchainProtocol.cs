using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Electrolyte.Messages;

namespace Electrolyte.Networking {
	public class BlockchainProtocol : NetworkProtocol {
		public WebClient Client;
		public Uri Address;

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
			return await Task.Run(() => Client.DownloadString(new Uri(Address, path)));
		}

		async Task<JToken> GetJsonAsync(string path) {
			return JToken.Parse(await GetContentAsync(path));
		}



		public async override Task<decimal> GetCurrencyConversionRateAsync(Money.CurrencyType c1, Money.CurrencyType c2) {
			if(c2 != Money.CurrencyType.FindByCode("BTC"))
				return await NextProtocol.GetCurrencyConversionRateAsync(c1, c2);

			return GetCurrencyConversionRate(c1, c2, await GetJsonAsync("/ticker"));
		}

		static decimal GetCurrencyConversionRate(Money.CurrencyType c1, Money.CurrencyType c2, JToken rates) {
			if(c2 != Money.CurrencyType.FindByCode("BTC"))
				throw new NotImplementedException();

			JToken rate = rates[c1.Code];
			if(rate == null)
				throw new Money.UnknownExchangeRateException();

			return rates[c1.Code]["last"].Value<decimal>();
		}
	}
}

