using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Electrolyte {
	public class Transaction {
		public static Int32 CurrentVersion = 1;

		public struct Input {
			//public Script ScriptSig;
			public Output Output;

			public Input(ref Output output) {
				Output = output;
			}
		}

		public struct Output {
			//public Script ScriptPubKey;
			public UInt64 Value;

			public Output(UInt64 value) {
				Value = value;
			}
		}

		public Int32 Version;

		public List<Input> Inputs;
		public List<Output> Outputs;

		public UInt64 Value {
			get { return Outputs.Select(o => o.Value).Sum(); }
		}

		public Transaction() {
			Version = CurrentVersion;
		}
	}
}

