using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Electrolyte.Primitives;
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

		public static long FeePerByteIncrement = 10000;
		public static long ByteIncrement = 1000;

		static CoinPicker() {
			Transaction baseTx = Transaction.Create(new List<Transaction.Output>(), new Dictionary<Address, long>(), new Dictionary<Address, ECKey>());
			using(MemoryStream stream = new MemoryStream()) {
				using(BinaryWriter writer = new BinaryWriter(stream)) {
					baseTx.WritePayload(writer);
					MinBytesForTx = (long)(stream.ToArray().LongLength * (1 + PercentError));
				}
			}

			Transaction.Input input = new Transaction.Input("23e90c875e2ed7a1ec01f5a80643879625b8aeb48b67db64c0f9edb8259240b6", 0, 0);
			input.ScriptSig = Script.FromString("3045022100f65c5e8c5d3b2386547a876db4ddb7bba1e57f9dbeaec9f3010516e453577fda02206f9df1a9262997263ac01be4342d0ade7057f5cceab4375fa1020ac7bfc5054b01 04ef96e3bccc8fff6b21d28e81f61c4a93cfe0f133214c9547c0d683a9fc12f529229c8d1ab20004c0f7f13961566b65492c6267fa452784c0724b4f542e4001f1");
			using(MemoryStream stream = new MemoryStream()) {
				using(BinaryWriter writer = new BinaryWriter(stream)) {
					input.Write(writer);
					ApproxBytesPerInput = (long)(stream.ToArray().LongLength * (1 + PercentError));
				}
			}

			Transaction.Output output = new Transaction.Output(Script.FromString("OP_DUP OP_HASH160 4da1b9632e160406693b961ff321402b22ce5452 OP_EQUALVERIFY OP_CHECKSIG"), 200000, baseTx, 0);
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

		public static long ApproxFeeForTx(long numberOfInputs, long numberOfOutputs) {
			return FeeForTx(ApproxSizeOfTx(numberOfInputs, numberOfOutputs));
		}

		public static long FeeForTx(long numberOfBytes) {
			return ((long)Math.Ceiling(numberOfBytes / (double)ByteIncrement) * FeePerByteIncrement);
		}

		public static long FeeForTx(Transaction tx) {
			using(MemoryStream stream = new MemoryStream()) {
				using(BinaryWriter writer = new BinaryWriter(stream)) {
					tx.WritePayload(writer);
					return FeeForTx(stream.ToArray().Length);
				}
			}
		}



		public static List<Transaction.Output> SelectInpoints(List<Transaction.Output> availableInpoints, Dictionary<Address, long> destinations, out long change) {
			long approxNumberOfInputs = SelectInpoints(availableInpoints, destinations, 0, out change).Count + 1;
			return SelectInpoints(availableInpoints, destinations, ApproxFeeForTx(approxNumberOfInputs, destinations.Count + 1), out change);
		}

		public static List<Transaction.Output> SelectInpoints(List<Transaction.Output> availableInpoints, Dictionary<Address, long> destinations, long fee, out long change) {
			return SelectInpoints(availableInpoints, destinations.Values.Sum(), fee, out change);
		}

		public static List<Transaction.Output> SelectInpoints(List<Transaction.Output> availableInpoints, long amountToSend, long fee, out long change) {
			List<Transaction.Output> selectedInpoints = new List<Transaction.Output>();
			foreach(Transaction.Output output in availableInpoints) {
				if(selectedInpoints.Select(o => o.Value).Sum() >= amountToSend + fee) break;
				selectedInpoints.Add(output);
			}

			long selectedTotal = selectedInpoints.Select(o => o.Value).Sum();

			if(selectedTotal < amountToSend + fee)
				throw new InsufficientFundsException();

			change = selectedTotal - (amountToSend + fee);

			return selectedInpoints;
		}
	}
}

