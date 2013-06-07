using System;
using System.Collections.Generic;
// using Electrolyte.Extensions;

namespace Electrolyte.Messages {
	public class Transaction {
		public static UInt32 CurrentVersion = 1;

		public class Input {
			//public Script ScriptSig;
			public Output Output;

			public Input(ref Output output) {
				Output = output;
			}
		}

		public class Output {
			//public Script ScriptPubKey;
			public UInt64 Value;

			public Output(UInt64 value) {
				Value = value;
			}
		}

		public UInt32 Version;

		public List<Input> Inputs;
		public List<Output> Outputs;

		public UInt64 Value {
			get {
				// TODO: Fix extension methods
				// return Outputs.Select(o => o.Value).Sum();
				UInt64 total = 0;
				foreach(Output output in Outputs) { total += output.Value; }
				return total;
			}
		}

		public Transaction() {
			Version = CurrentVersion;
		}
	}
}

