using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using Electrolyte;
using Electrolyte.Primitives;
using Electrolyte.Helpers;

namespace Electrolyte.Messages {
	public partial class Transaction  {
		public class Input {
			public Transaction Transaction;
			public UInt32 Index;
			public Script ScriptSig;

			public byte[] PrevTransactionHashBytes;
			public string PrevTransactionHash {
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
				PrevTransactionHashBytes = BinaryHelpers.HexToByteArray(data["prev_out"]["hash"].Value<string>());
				OutpointIndex = data["prev_out"]["n"].Value<uint>();
				ScriptSig = Script.FromString(data["scriptSig"].Value<string>());
			}

			public static Input Read(BinaryReader reader, Transaction transaction = null, UInt32 index = 0) {
				var input = new Input();
				input.ReadPayload(reader);
				input.Transaction = transaction;
				return input;
			}

			public static Input FromJson(string json, Transaction transaction = null, UInt32 index = 0) {
				return FromJson(JToken.Parse(json), transaction);
			}

			public static Input FromJson(JToken data, Transaction transaction = null, UInt32 index = 0) {
				var input = new Input();
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
				var output = new Output();
				output.ReadPayload(reader);
				output.Transaction = transaction;
				output.Index = index;
				return output;
			}

			public static Output FromJson(string json, Transaction transaction = null, UInt32 index = 0) {
				return FromJson(JToken.Parse(json), transaction, index);
			}

			public static Output FromJson(JToken data, Transaction transaction = null, UInt32 index = 0) {
				var output = new Output();
				output.ReadFromJson(data);
				output.Transaction = transaction;
				output.Index = index;
				return output;
			}
		}
	}
}


