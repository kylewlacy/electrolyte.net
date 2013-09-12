using System;
using Electrolyte;

namespace Electrolyte.Messages {
	public partial class Transaction {
		public struct Info {
			public string Hash;
			public ulong Height;

			public Info(string hash, ulong height) {
				Hash = hash;
				Height = height;
			}
		}

		public struct Delta {
			public Transaction Transaction;
			public Money Value;

			public Delta(Transaction tx, Money value) {
				Transaction = tx;
				Value = value;
			}

			public override string ToString() {
				return String.Format("{0}{1}", Value > Money.Zero("BTC") ? "+" : "", Value);
			}
		}
	}
}


