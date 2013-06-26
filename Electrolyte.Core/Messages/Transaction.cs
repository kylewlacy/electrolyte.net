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

			protected void ReadFromJson(JToken data) {
				Console.WriteLine(data["prev_out"].ToString());
				string hash = data["prev_out"]["hash"].Value<string>();
				PrevTransactionHash = new byte[hash.Length / 2];

				for(int i = 0; i < PrevTransactionHash.Length; i++) {
					PrevTransactionHash[i] = Convert.ToByte(hash.Substring(i * 2, 2), 16);
				}
				OutpointIndex = data["prev_out"]["n"].Value<int>();

				ScriptSig = Script.FromString(data["scriptSig"].Value<string>());
			}

			public static Input Read(BinaryReader reader) {
				Input input = new Input();
				input.ReadPayload(reader);
				return input;
			}

			public static Input FromJson(string json) {
				return FromJson(JToken.Parse(json));
			}

			public static Input FromJson(JToken data) {
				Input input = new Input();
				input.ReadFromJson(data);
				return input;
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

			protected void ReadFromJson(JToken data) {
				// TODO: Move conversion to a class
				decimal btc = Decimal.Parse(data["value"].Value<string>());
				Value = (Int64)Math.Floor(btc * 100000000); // TODO: Is flooring the proper way to round?
				Console.WriteLine("BTC: {0}\nSAT: {1}", btc, Value);

				ScriptPubKey = Script.FromString(data["scriptPubKey"].Value<string>());
			}

			public static Output Read(BinaryReader reader) {
				Output output = new Output();
				output.ReadPayload(reader);
				return output;
			}

			public static Output FromJson(string json) {
				return FromJson(JToken.Parse(json));
			}

			public static Output FromJson(JToken data) {
				Output output = new Output();
				output.ReadFromJson(data);
				return output;
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

		protected void ReadFromJson(JToken data) {
			Version = data["ver"].Value<uint>();

			ulong inputCount = data["vin_sz"].Value<ulong>();

			JToken[] inputs = data["in"].ToArray();
			for(ulong i = 0; i < inputCount; i++)
				Inputs.Add(Input.FromJson(inputs[i]));

			ulong outputCount = data["vout_sz"].Value<ulong>();

			JToken[] outputs = data["out"].ToArray();
			for(ulong i = 0; i < outputCount; i++)
				Outputs.Add(Output.FromJson(outputs[i]));
		}


		public static Transaction FromJson(string json) {
			return FromJson(JToken.Parse(json));
		}

		static Transaction FromJson(JToken data) {
			Transaction tx = new Transaction();
			tx.ReadFromJson(data);
			return tx;
		}
	}
}

