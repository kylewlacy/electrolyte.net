using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Electrolyte.Primitives;
using Electrolyte.Extensions;
using Electrolyte.Messages;

namespace Electrolyte.Helpers {
	public static class CoinPicker {
		public class InsufficientFundsException : InvalidOperationException {
			public InsufficientFundsException () { }
			public InsufficientFundsException (string message) : base(message) { }
			public InsufficientFundsException (string message, Exception inner) : base(message, inner) { }
		}

		public static float PercentError = 0.05f;
		public static long ApproxBytesPerInput;
		public static long ApproxBytesPerOutput;
		public static long MinBytesForTx;

		public static Money FeePerByteIncrement = Money.Create(0.0001m, "BTC");
		public static long ByteIncrement = 1000;

		static CoinPicker() {
			Transaction baseTx = Transaction.Create(new List<Transaction.Output>(), new Dictionary<Address, Money>(), new Dictionary<Address, ECKey>());
			MinBytesForTx = (long)(baseTx.ToByteArray().LongLength * (1 + PercentError));

			Transaction.Input input = new Transaction.Input("23e90c875e2ed7a1ec01f5a80643879625b8aeb48b67db64c0f9edb8259240b6", 0, 0);
			input.ScriptSig = Script.FromString("3045022100f65c5e8c5d3b2386547a876db4ddb7bba1e57f9dbeaec9f3010516e453577fda02206f9df1a9262997263ac01be4342d0ade7057f5cceab4375fa1020ac7bfc5054b01 04ef96e3bccc8fff6b21d28e81f61c4a93cfe0f133214c9547c0d683a9fc12f529229c8d1ab20004c0f7f13961566b65492c6267fa452784c0724b4f542e4001f1");
			using(MemoryStream stream = new MemoryStream()) {
				using(BinaryWriter writer = new BinaryWriter(stream)) {
					input.Write(writer);
					ApproxBytesPerInput = (long)(stream.ToArray().LongLength * (1 + PercentError));
				}
			}

			Transaction.Output output = new Transaction.Output(Script.FromString("OP_DUP OP_HASH160 4da1b9632e160406693b961ff321402b22ce5452 OP_EQUALVERIFY OP_CHECKSIG"), Money.Create(0.002m, "BTC"), baseTx, 0);
			using(MemoryStream stream = new MemoryStream()) {
				using(BinaryWriter writer = new BinaryWriter(stream)) {
					output.Write(writer);
					ApproxBytesPerOutput = (long)(stream.ToArray().LongLength * (1 + PercentError));
				}
			}
		}

		public static long ApproxSizeOfTx(long numberOfInputs, long numberOfOutputs) {
			return (numberOfInputs * ApproxBytesPerInput) + (numberOfOutputs * ApproxBytesPerOutput) + MinBytesForTx;
		}

		public static Money ApproxFeeForTx(long numberOfInputs, long numberOfOutputs) {
			return FeeForTx(ApproxSizeOfTx(numberOfInputs, numberOfOutputs));
		}

		public static Money FeeForTx(long numberOfBytes) {
			return  FeePerByteIncrement * ((long)Math.Ceiling(numberOfBytes / (double)ByteIncrement));
		}

		public static Money FeeForTx(Transaction tx) {
			return FeeForTx(tx.ToByteArray().Length);
		}



		public static List<Transaction.Output> SelectInpoints(List<Transaction.Output> availableInpoints, Dictionary<Address, Money> destinations, out Money change) {
			long approxNumberOfInputs = SelectInpoints(availableInpoints, destinations, new Money(0, "BTC"), out change).Count + 1;
			return SelectInpoints(availableInpoints, destinations, ApproxFeeForTx(approxNumberOfInputs, destinations.Count + 1), out change);
		}

		public static List<Transaction.Output> SelectInpoints(List<Transaction.Output> availableInpoints, Dictionary<Address, Money> destinations, Money fee, out Money change) {
			return SelectInpoints(availableInpoints, destinations.Values.Sum(), fee, out change);
		}

		public static List<Transaction.Output> SelectInpoints(List<Transaction.Output> availableInpoints, Money amountToSend, Money fee, out Money change) {
			List<Transaction.Output> selectedInpoints = new List<Transaction.Output>();
			foreach(Transaction.Output output in availableInpoints) {
				if(selectedInpoints.Select(o => o.Value).Sum() >= amountToSend + fee) break;
				selectedInpoints.Add(output);
			}

			Money selectedTotal = selectedInpoints.Select(o => o.Value).Sum();

			if(selectedTotal < amountToSend + fee)
				throw new InsufficientFundsException();

			change = selectedTotal - (amountToSend + fee);

			return selectedInpoints;
		}
	}
}

