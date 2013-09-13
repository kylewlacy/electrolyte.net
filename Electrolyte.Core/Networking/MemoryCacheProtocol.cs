using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Electrolyte.Primitives;
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

		readonly Dictionary<string, Transaction> TransactionCache = new Dictionary<string, Transaction>();
		static readonly SemaphoreLite txCacheLock = new SemaphoreLite();

		readonly Dictionary<Money.CurrencyType, Dictionary<Money.CurrencyType, ExchangeRateInfo>> ExchangeRateCache = new Dictionary<Money.CurrencyType, Dictionary<Money.CurrencyType,ExchangeRateInfo>>();
		static readonly SemaphoreLite exchangeRateLock = new SemaphoreLite();

		public async override Task<Transaction> GetTransactionAsync(Transaction.Info info) {
			await txCacheLock.WaitAsync();
			try {
				if(!TransactionCache.ContainsKey(info.Hash))
					TransactionCache.Add(info.Hash, await base.GetTransactionAsync(info));
			}
			finally {
				txCacheLock.Release();
			}

			return TransactionCache[info.Hash];
		}

		public override async Task<decimal> GetExchangeRateAsync(Money.CurrencyType c1, Money.CurrencyType c2) {
			await exchangeRateLock.WaitAsync();
			try {
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

				decimal rate = await base.GetExchangeRateAsync(c1, c2);
				ExchangeRateCache[c1].Add(c2, new ExchangeRateInfo(rate));
				return rate;
			}
			finally {
				exchangeRateLock.Release();
			}
		}
	}
}

