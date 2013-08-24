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
	public class MemoryCacheProtocol : NetworkProtocol {
		struct ExchangeRateInfo {
			public decimal Rate;
			public DateTime CreationTime;

			public TimeSpan Age {
				get { return DateTime.Now - CreationTime; }
			}

			public ExchangeRateInfo(decimal rate) {
				Rate = rate;
				CreationTime = DateTime.Now;
			}
		}

		public static TimeSpan MaxExchangeRateAge = new TimeSpan(0, 5, 0);

		Dictionary<string, Transaction> TransactionCache = new Dictionary<string, Transaction>();
		Dictionary<Money.CurrencyType, Dictionary<Money.CurrencyType, ExchangeRateInfo>> ExchangeRateCache = new Dictionary<Money.CurrencyType, Dictionary<Money.CurrencyType,ExchangeRateInfo>>();

		public async override Task<Transaction> GetTransactionAsync(TransactionInfo info) {
			if(!TransactionCache.ContainsKey(info.Hex))
				TransactionCache.Add(info.Hex, await NextProtocol.GetTransactionAsync(info));

			return TransactionCache[info.Hex];
		}

		public override async Task<decimal> GetExchangeRateAsync(Money.CurrencyType c1, Money.CurrencyType c2) {
			if(ExchangeRateCache.ContainsKey(c2)) {
				return 1m / (await GetExchangeRateAsync(c2, c1));
			}

			if(!ExchangeRateCache.ContainsKey(c1))
				ExchangeRateCache.Add(c1, new Dictionary<Money.CurrencyType, ExchangeRateInfo>());

			if(ExchangeRateCache[c1].ContainsKey(c2)) {
				if(ExchangeRateCache[c1][c2].Age <= MaxExchangeRateAge)
					return ExchangeRateCache[c1][c2].Rate;

				ExchangeRateCache[c1].Remove(c2);
			}

			decimal rate = await NextProtocol.GetExchangeRateAsync(c1, c2);
			ExchangeRateCache[c1].Add(c2, new ExchangeRateInfo(rate));
			return rate;
		}
	}
}

