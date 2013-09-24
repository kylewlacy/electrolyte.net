using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Electrolyte.Primitives;
using Electrolyte.Messages;
using Electrolyte.Helpers;

namespace Electrolyte.Networking {
	public class MemoryCacheProtocol : CacheProtocol {
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

		readonly Dictionary<Address, Money> BalanceCache = new Dictionary<Address, Money>();
		static readonly SemaphoreLite balanceCacheLock = new SemaphoreLite();

		public async override Task<Transaction> GetTransactionAsync(Transaction.Info info) {
			await txCacheLock.WaitAsync();
			try {
				if(!TransactionCache.ContainsKey(info.Hash))
					TransactionCache.Add(info.Hash, await base.GetTransactionAsync(info));
				return TransactionCache[info.Hash];
			}
			finally {
				txCacheLock.Release();
			}
		}

		public override async Task<Money> GetAddressBalanceAsync(Address address, ulong startHeight = 0) {
			Money balance = await base.GetAddressBalanceAsync(address, startHeight);
			if(!BalanceCache.ContainsKey(address))
				BalanceCache.Add(address, balance);
			else
				BalanceCache[address] = balance;

			return balance;
		}

		public override async Task<Money> GetCachedBalanceAsync(Address address, ulong startHeight = 0) {
			await balanceCacheLock.WaitAsync();
			try {
				if(!BalanceCache.ContainsKey(address))
					BalanceCache.Add(address, await base.GetCachedBalanceAsync(address, startHeight));
				return BalanceCache[address];
			}
			finally {
				balanceCacheLock.Release();
			}
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

