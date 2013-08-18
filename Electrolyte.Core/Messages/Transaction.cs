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

			// TODO: Make this consistent with `Transaction.Hash`
			public byte[] PrevTransactionHash;
			public Transaction PreviousTransaciton;

			public UInt32 OutpointIndex;
			public Output Outpoint {
				get { return PreviousTransaciton.Outputs[(int)OutpointIndex]; }
			}
			public UInt32 Sequence;

			public Address Sender {
				get {
					// TODO: Check ScriptSig format beforehand

					// TODO: Move this out to a method (`Script.Data[1]`)
					string pubKeyHex = ScriptSig.ToString().Split(' ')[1];
					// TODO: Rewrite faster
					byte[] pubKey = Enumerable.Range(0, pubKeyHex.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(pubKeyHex.Substring(x, 2), 16)).ToArray();

					using(RIPEMD160 ripemd = RIPEMD160.Create()) {
						using(SHA256 sha256 = SHA256.Create()) {
							byte[] pubKeyHash = ripemd.ComputeHash(sha256.ComputeHash(pubKey));
							// TODO: Move current network byte out somewhere
							byte[] addressBytes = ArrayHelpers.ConcatArrays(new byte[] { 0x00 }, pubKeyHash);
							return new Address(Base58.EncodeWithChecksum(addressBytes));
						}
					}
				}
			}

			protected Input() {	}

			public void Write(BinaryWriter writer) {
				writer.Write(PrevTransactionHash);
				writer.Write(OutpointIndex);

				new VarInt(ScriptSig.Execution.Count).Write(writer);
				writer.Write(ScriptSig.Execution.ToArray()); // TODO: Move this logic to Script class

				writer.Write(Sequence);
			}

			protected void ReadPayload(BinaryReader reader) {
				PrevTransactionHash = reader.ReadBytes(32);
				OutpointIndex = reader.ReadUInt32();

				UInt64 scriptLength = VarInt.Read(reader).Value;
				ScriptSig = new Script(reader.ReadBytes((int)scriptLength));

				Sequence = reader.ReadUInt32();
			}

			protected void ReadFromJson(JToken data) {
				Console.WriteLine(data["prev_out"]);
				string hash = data["prev_out"]["hash"].Value<string>();
				PrevTransactionHash = new byte[hash.Length / 2];

				for(int i = 0; i < PrevTransactionHash.Length; i++) {
					PrevTransactionHash[i] = Convert.ToByte(hash.Substring(i * 2, 2), 16);
				}
				OutpointIndex = data["prev_out"]["n"].Value<uint>();

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

			public Address Recipient {
				get {
					// TODO: Check ScriptPubKey format beforehand

					// TODO: Move this out to a method (`Script.Data[1]`)
					string pubKeyHex = ScriptPubKey.ToString().Split(' ')[2];
					// TODO: Rewrite faster
					byte[] pubKeyHash = Enumerable.Range(0, pubKeyHex.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(pubKeyHex.Substring(x, 2), 16)).ToArray();

					// TODO: Move current network byte out somewhere
					byte[] addressBytes = ArrayHelpers.ConcatArrays(new byte[] { 0x00 }, pubKeyHash);
					return new Address(Base58.EncodeWithChecksum(addressBytes));
				}
			}

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

		public List<Address> Senders {
			get { return Inputs.Select(i => i.Sender).ToList(); }
		}

		public List<Address> Recipients {
			get { return Outputs.Select(o => o.Recipient).ToList(); }
		}

		public Int64 Value {
			get { return Outputs.Select(o => o.Value).Sum(); }
		}

		public UInt32 LockTime; // TODO: Use a struct

		public override string ExpectedCommand {
			get { return "tx"; }
		}

		public string Hash {
			get {
				using(MemoryStream stream = new MemoryStream()) {
					using(BinaryWriter writer = new BinaryWriter(stream)) {
						WritePayload(writer);
						using(SHA256 sha256 = SHA256.Create()) {
							return BitConverter.ToString(sha256.ComputeHash(sha256.ComputeHash(stream.ToArray())).Reverse().ToArray()).Replace("-", "").ToLower();
						}
					}
				}
			}
		}

		public byte[] InputHash(SigHash hashType, Script subScript, int inputIndex) {
			if(hashType.HasFlag(SigHash.None))
				throw new NotImplementedException();
			if(hashType.HasFlag(SigHash.Single))
				throw new NotImplementedException();
			if(hashType.HasFlag(SigHash.AnyoneCanPay))
				throw new NotImplementedException();

			Transaction copy = new Transaction();
			using(MemoryStream originalStream = new MemoryStream()) {
				using(BinaryWriter writer = new BinaryWriter(originalStream))
					WritePayload(writer);

				using(MemoryStream copyStream = new MemoryStream(originalStream.ToArray())) {
					using(BinaryReader reader = new BinaryReader(copyStream))
						copy.ReadPayload(reader);
				}
			}

			foreach(Input input in copy.Inputs) {
				input.ScriptSig = new Script();
			}

			copy.Inputs[inputIndex].ScriptSig = subScript;

			List<byte> verify = new List<byte>();
			using(MemoryStream stream = new MemoryStream()) {
				using(BinaryWriter writer = new BinaryWriter(stream))
					copy.WritePayload(writer);

				verify.AddRange(stream.ToArray());
			}

			verify.AddRange(BitConverter.GetBytes((UInt32)hashType));

			using(SHA256 sha256 = SHA256.Create())
				return sha256.ComputeHash(sha256.ComputeHash(verify.ToArray()));
		}

		public bool SigIsValid(byte[] pubKey, byte[] sigWithHashType, Script subScript, int inputIndex) {
			byte[] sig = ArrayHelpers.SubArray(sigWithHashType, 0, sigWithHashType.Length - 1);
			SigHash hashType = (SigHash)sigWithHashType[sigWithHashType.Length - 1];

			return ECKey.Verify(InputHash(hashType, subScript, inputIndex), sig, pubKey);
		}

		public byte[] GenerateInputSignature(ECKey privateKey, SigHash hashType, Script subScript, int inputIndex) {
			if(hashType.HasFlag(SigHash.None))
				throw new NotImplementedException();
			if(hashType.HasFlag(SigHash.Single))
				throw new NotImplementedException();
			if(hashType.HasFlag(SigHash.AnyoneCanPay))
				throw new NotImplementedException();

			return ArrayHelpers.ConcatArrays(privateKey.Sign(InputHash(hashType, subScript, inputIndex)), new byte[] { (byte)hashType });
		}

		public override void ReadPayload(BinaryReader reader) {
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

