using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using Electrolyte;
using Electrolyte.Extensions;
using Electrolyte.Primitives;
using Electrolyte.Helpers;

namespace Electrolyte.Messages {
	public class Transaction : Message<Transaction> {
		public static UInt32 CurrentVersion = 1;
		public static Address.Network CurrentNetwork = Address.Network.Bitcoin;

		public class Input {
			public Transaction Transaction;
			public UInt32 Index;
			public Script ScriptSig;

			public byte[] PrevTransactionHashBytes;
			public string PrevTransactionHash {
//				get { return BitConverter.ToString(PrevTransactionHashBytes.Reverse().ToArray()).Replace("-", "").ToLower(); }
				get { return BinaryHelpers.ByteArrayToHex(PrevTransactionHashBytes.Reverse().ToArray()); }
			}
			public Transaction PreviousTransaciton;

			public UInt32 OutpointIndex;
			Output _outpoint;
			public Output Outpoint {
				get {
					_outpoint = _outpoint ?? PreviousTransaciton.Outputs[(int)OutpointIndex];
					return _outpoint;
				}
			}
			public UInt32 Sequence;

			public Address Sender {
				get {
					// TODO: Check ScriptSig format beforehand

					// TODO: Move this out to a method (`Script.Data[1]`)
					string pubKeyHex = ScriptSig.ToString().Split(' ')[1];

//					byte[] pubKey = Enumerable.Range(0, pubKeyHex.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(pubKeyHex.Substring(x, 2), 16)).ToArray();
					byte[] pubKey = BinaryHelpers.HexToByteArray(pubKeyHex);

					using(RIPEMD160 ripemd = RIPEMD160.Create()) {
						using(SHA256 sha256 = SHA256.Create()) {
							byte[] pubKeyHash = ripemd.ComputeHash(sha256.ComputeHash(pubKey));
							byte[] addressBytes = ArrayHelpers.ConcatArrays(new byte[] { Address.BytePrefixForNetwork(CurrentNetwork) }, pubKeyHash);
							return new Address(Base58.EncodeWithChecksum(addressBytes));
						}
					}
				}
			}

			protected Input() {	}
			public Input(string prevTxHash, UInt32 outpointIndex, UInt32 index, UInt32 sequence = 0xFFFFFFFF) {
//				PrevTransactionHashBytes = Enumerable.Range(0, prevTxHash.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(prevTxHash.Substring(x, 2), 16)).Reverse().ToArray();
				PrevTransactionHashBytes = BinaryHelpers.HexToByteArray(prevTxHash).Reverse().ToArray();
				OutpointIndex = outpointIndex;
				Index = index;
				Sequence = sequence;
				ScriptSig = new Script(new byte[] { });
			}

			public Input(Output outpoint, UInt32 index, UInt32 sequence = 0xFFFFFFFF) : this(outpoint.Transaction.Hash, outpoint.Index, index, sequence) {
				_outpoint = outpoint;
			}

			public void Write(BinaryWriter writer) {
				writer.Write(PrevTransactionHashBytes);
				writer.Write(OutpointIndex);

				new VarInt(ScriptSig.Execution.Count).Write(writer);
				writer.Write(ScriptSig.Execution.ToArray()); // TODO: Move this logic to Script class

				writer.Write(Sequence);
			}

			protected void ReadPayload(BinaryReader reader) {
				PrevTransactionHashBytes = reader.ReadBytes(32);
				OutpointIndex = reader.ReadUInt32();

				UInt64 scriptLength = VarInt.Read(reader).Value;
				ScriptSig = new Script(reader.ReadBytes((int)scriptLength));

				Sequence = reader.ReadUInt32();
			}

			protected void ReadFromJson(JToken data) {
				Console.WriteLine(data["prev_out"]);
				string hash = data["prev_out"]["hash"].Value<string>();
				PrevTransactionHashBytes = new byte[hash.Length / 2];

				for(int i = 0; i < PrevTransactionHashBytes.Length; i++) {
					PrevTransactionHashBytes[i] = Convert.ToByte(hash.Substring(i * 2, 2), 16);
				}
				OutpointIndex = data["prev_out"]["n"].Value<uint>();

				ScriptSig = Script.FromString(data["scriptSig"].Value<string>());
			}

			public static Input Read(BinaryReader reader, Transaction transaction = null, UInt32 index = 0) {
				Input input = new Input();
				input.ReadPayload(reader);
				input.Transaction = transaction;
				return input;
			}

			public static Input FromJson(string json, Transaction transaction = null, UInt32 index = 0) {
				return FromJson(JToken.Parse(json), transaction);
			}

			public static Input FromJson(JToken data, Transaction transaction = null, UInt32 index = 0) {
				Input input = new Input();
				input.ReadFromJson(data);
				input.Transaction = transaction;
				return input;
			}
		}

		public class Output {
			public Transaction Transaction;
			public UInt32 Index;
			public Script ScriptPubKey;
			public Money Value;

			public Address Recipient {
				get {
					// TODO: Check ScriptPubKey format beforehand

					// TODO: Move this out to a method (`Script.Data[1]`)
					string pubKeyHex = ScriptPubKey.ToString().Split(' ')[2];

//					byte[] pubKeyHash = Enumerable.Range(0, pubKeyHex.Length).Where(x => x % 2 == 0).Select(x => Convert.ToByte(pubKeyHex.Substring(x, 2), 16)).ToArray();
					byte[] pubKeyHash = BinaryHelpers.HexToByteArray(pubKeyHex);
					byte[] addressBytes = ArrayHelpers.ConcatArrays(new byte[] { Address.BytePrefixForNetwork(CurrentNetwork) }, pubKeyHash);
					return new Address(Base58.EncodeWithChecksum(addressBytes));
				}
			}

			public Output(Script scriptPubKey, Money value, Transaction transaction = null, UInt32 index = 0) {
				ScriptPubKey = scriptPubKey;
				Value = value;
				Transaction = transaction;
				Index = index;
			}

			protected Output() { }

			protected void ReadPayload(BinaryReader reader) {
				Value = new Money(reader.ReadInt64(), "BTC");

				UInt64 scriptLength = VarInt.Read(reader).Value;
				ScriptPubKey = new Script(reader.ReadBytes((int)scriptLength));
			}

			public void Write(BinaryWriter writer) {
				writer.Write(Value.Cents);

				new VarInt(ScriptPubKey.Execution.Count).Write(writer);
				writer.Write(ScriptPubKey.Execution.ToArray());
			}

			protected void ReadFromJson(JToken data) {
				Value = Money.Create(Decimal.Parse(data["value"].Value<string>()), "BTC");
//				Console.WriteLine("BTC: {0}\nSAT: {1}", btc, Value);

				ScriptPubKey = Script.FromString(data["scriptPubKey"].Value<string>());
			}

			public static Output Read(BinaryReader reader, Transaction transaction = null, UInt32 index = 0) {
				Output output = new Output();
				output.ReadPayload(reader);
				output.Transaction = transaction;
				output.Index = index;
				return output;
			}

			public static Output FromJson(string json, Transaction transaction = null, UInt32 index = 0) {
				return FromJson(JToken.Parse(json), transaction, index);
			}

			public static Output FromJson(JToken data, Transaction transaction = null, UInt32 index = 0) {
				Output output = new Output();
				output.ReadFromJson(data);
				output.Transaction = transaction;
				output.Index = index;
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

		public Money Value {
			get { return Outputs.Select(o => o.Value).Sum(); }
		}

		public Money Fee {
			get { return Inputs.Select(i => i.Outpoint.Value).Sum() - Value;  }
		}

		public bool IncludesStandardFee {
			get { return CoinPicker.FeeForTx(this) == Fee; }
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
//							return BitConverter.ToString(sha256.ComputeHash(sha256.ComputeHash(stream.ToArray())).Reverse().ToArray()).Replace("-", "").ToLower();
							return BinaryHelpers.ByteArrayToHex(sha256.ComputeHash(sha256.ComputeHash(stream.ToArray())).Reverse().ToArray());
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
				Inputs.Add(Input.Read(reader, this, (UInt32)i));
			}

			UInt64 outputCount = VarInt.Read(reader).Value;
			for(ulong i = 0; i < outputCount; i++) {
				Outputs.Add(Output.Read(reader, this, (UInt32)i));
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
				Inputs.Add(Input.FromJson(inputs[i], this, (UInt32)i));

			ulong outputCount = data["vout_sz"].Value<ulong>();

			JToken[] outputs = data["out"].ToArray();
			for(ulong i = 0; i < outputCount; i++)
				Outputs.Add(Output.FromJson(outputs[i], this, (UInt32)i));
		}

		
		public byte[] ToByteArray() {
			using(MemoryStream stream = new MemoryStream()) {
				using(BinaryWriter writer = new BinaryWriter(stream)) {
					WritePayload(writer);
					return stream.ToArray();
				}
			}
		}

		public string ToHex() {
			return BinaryHelpers.ByteArrayToHex(ToByteArray());
		}

		public static Transaction FromByteArray(byte[] bytes) {
			using(MemoryStream stream = new MemoryStream(bytes)) {
				using(BinaryReader reader = new BinaryReader(stream)) {
					Transaction tx = new Transaction();
					tx.ReadPayload(reader);
					return tx;
				}
			}
		}

		public static Transaction FromHex(string hex) {
			return FromByteArray(BinaryHelpers.HexToByteArray(hex));
		}



		public static Transaction FromJson(string json) {
			return FromJson(JToken.Parse(json));
		}

		static Transaction FromJson(JToken data) {
			Transaction tx = new Transaction();
			tx.ReadFromJson(data);
			return tx;
		}

		public static Transaction Create(List<Output> inpoints, Dictionary<Address, Money> destinations, Dictionary<Address, ECKey> privateKeys) {
			Transaction tx = new Transaction();

			tx.Version = CurrentVersion;

			int outputIndex = 0;
			foreach(KeyValuePair<Address, Money> destination in destinations) {
				Script pkScript = Script.Create(Op.Dup, Op.Hash160, ArrayHelpers.SubArray(Base58.DecodeWithChecksum(destination.Key.ID), 1), Op.EqualVerify, Op.CheckSig);
				tx.Outputs.Add(new Output(pkScript, destination.Value, tx, (uint)outputIndex));
				outputIndex++;
			}

			foreach(Output inpoint in inpoints) {
				tx.Inputs.Add(new Input(inpoint, (uint)tx.Inputs.Count));
			}

			foreach(Input input in tx.Inputs) {
				ECKey key = privateKeys[input.Outpoint.Recipient];
//				byte[] sig = tx.GenerateInputSignature(key, SigHash.All, inpoint.ScriptPubKey.SubScript, inputIndex);
				byte[] sig = tx.GenerateInputSignature(key, SigHash.All, input.Outpoint.ScriptPubKey, (int)input.Index);
				input.ScriptSig = Script.Create(sig, key.PubKey);
			}

			tx.LockTime = 0;

			return tx;
		}
	}
}

