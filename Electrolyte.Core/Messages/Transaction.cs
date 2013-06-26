using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using Electrolyte;
using Electrolyte.Primitives;
using Electrolyte.Helpers;

namespace Electrolyte.Messages {
	public class Transaction : Message<Transaction> {
		public static UInt32 CurrentVersion = 1;

		public class Input {
			public Script ScriptSig;

			public byte[] PrevTransactionHash;
			public Transaction PreviousTransaciton;

			protected int OutpointIndex;
			public Output Outpoint {
				get { return PreviousTransaciton.Outputs[OutpointIndex]; }
			}
			public UInt32 Sequence;

			protected Input() {	}

			public void Write(BinaryWriter writer) {
				writer.Write(PrevTransactionHash);

				new VarInt(ScriptSig.Execution.Count).Write(writer);
				writer.Write(ScriptSig.Execution.ToArray()); // TODO: Move this logic to Script class

				writer.Write(Sequence);
			}

			protected void ReadPayload(BinaryReader reader) {
				PrevTransactionHash = reader.ReadBytes(36);

				UInt64 scriptLength = VarInt.Read(reader).Value;
				ScriptSig = new Script(reader.ReadBytes((int)scriptLength));

				Sequence = reader.ReadUInt32();
			}

			public void Write(BinaryWriter writer) {
				writer.Write(PrevTransactionHash);

				new VarInt(ScriptSig.Execution.Count).Write(writer);
				writer.Write(ScriptSig.Execution.ToArray()); // TODO: Move this logic to Script class

				writer.Write(Sequence);
			public static Input Read(BinaryReader reader) {
				Input input = new Input();
				input.ReadPayload(reader);
				return input;
			}
			}
		}

		public class Output {
			public Script ScriptPubKey;
			public Int64 Value;

			protected Output() {

			}

			protected void ReadPayload(BinaryReader reader) {
				Value = reader.ReadInt64();

				UInt64 scriptLength = VarInt.Read(reader).Value;
				ScriptPubKey = new Script(reader.ReadBytes((int)scriptLength));
			}

			public void Write(BinaryWriter writer) {
				writer.Write(Value);

				new VarInt(ScriptPubKey.Execution.Count).Write(writer);
				writer.Write(ScriptPubKey.Execution.ToArray());
			}
		}

		public UInt32 Version;

		public List<Input> Inputs = new List<Input>();
		public List<Output> Outputs = new List<Output>();

		public Int64 Value {
			get { return Outputs.Select(o => o.Value).Sum(); }
		}

		public UInt32 LockTime; // TODO: Use a struct

		public override string ExpectedCommand {
			get { return "tx"; }
		}

		public bool SigIsValid(byte[] pubKey, byte[] sigWithHashType, Script scriptSig) {
//			uint hashType = sigWithHashType[sigWithHashType.Count() - 1];
//			byte[] sig = ArrayHelpers.SubArray(sigWithHashType, 0, sigWithHashType.Count() - 2);
//			Transaction copy;
//
//			using(MemoryStream stream = new MemoryStream()) {
//				using(BinaryWriter writer = new BinaryWriter(stream))
//					WritePayload(writer);
//
//				copy = Transaction.Read(new BinaryReader(stream));
//			}
//
//			for(int i = 0; i < copy.Inputs.Count; i++)
//				copy.Inputs[i].ScriptSig = new Script();
//
//			copy.Inputs[scriptSig.InputIndex].ScriptSig = new Script(scriptSig.SubScript);
//			byte[] data;
//
//			using(MemoryStream stream = new MemoryStream()) {
//				using(BinaryWriter writer = new BinaryWriter(stream))
//					copy.WritePayload(writer);
//
//				using(SHA256 sha256 = SHA256.Create())
//					data = sha256.ComputeHash(sha256.ComputeHash(ArrayHelpers.ConcatArrays(stream.ToArray(), BitConverter.GetBytes(hashType))));
//			}
//
//			ISigner signer = SignerUtilities.GetSigner("ECDSA");
//			signer.Init(false, PublicKeyFactory.CreateKey(pubKey));
//			signer.BlockUpdate(data, 0, data.Length);
//			return signer.VerifySignature(sig);
			return true;
		}

		protected override void ReadPayload(BinaryReader reader) {
			Version = reader.ReadUInt32();

			UInt64 inputCount = VarInt.Read(reader).Value;
			for(ulong i = 0; i < inputCount; i++) {
				Inputs.Add(Input.Read(reader));
			}

			UInt64 outputCount = VarInt.Read(reader).Value;
			for(ulong i = 0; i < outputCount; i++) {
				Outputs.Add(Output.Read(reader));
			}

			LockTime = reader.ReadUInt32();
		}

		public override void WritePayload(BinaryWriter writer) {
			writer.Write(Version);

			new VarInt(Inputs.Count).Write(writer);
			foreach(Input input in Inputs) {
				input.Write(writer);
			}

			new VarInt(Outputs.Count).Write(writer);
			foreach(Output output in Outputs) {
				output.Write(writer);
			}

			writer.Write(LockTime);
		}
	}
}

