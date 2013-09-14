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
	public partial class Transaction : Message<Transaction> {
		public class DustException : ArgumentException {
			public DustException() { }
			public DustException(string message) : base(message) { }
			public DustException(string message, Exception inner) : base(message, inner) { }
		}

		public static UInt32 CurrentVersion = 1;
		public static Address.Network CurrentNetwork = Address.Network.Bitcoin;

		public static Money DustValue = CoinPicker.MinimumFee * 0.543m;

		public UInt32 Version;
		public ulong? Height;

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

		public UInt32 LockTime;

		public override string ExpectedCommand {
			get { return "tx"; }
		}

		public string Hash {
			get {
				using(SHA256 sha256 = SHA256.Create()) {
					return BinaryHelpers.ByteArrayToHex(sha256.ComputeHash(sha256.ComputeHash(ToByteArray())).Reverse().ToArray());
				}
			}
		}

		public Transaction() { }

		public Transaction(ulong height) {
			Height = height;
		}

		public byte[] InputHash(SigHash hashType, Script subScript, int inputIndex) {
			if(hashType.HasFlag(SigHash.None))
				throw new NotImplementedException();
			if(hashType.HasFlag(SigHash.Single))
				throw new NotImplementedException();
			if(hashType.HasFlag(SigHash.AnyoneCanPay))
				throw new NotImplementedException();

			Transaction copy = Transaction.FromByteArray(ToByteArray());

			foreach(Input input in copy.Inputs) {
				input.ScriptSig = new Script();
			}

			copy.Inputs[inputIndex].ScriptSig = subScript;

			var verify = new List<byte>();
			verify.AddRange(copy.ToByteArray());
			verify.AddRange(BitConverter.GetBytes((UInt32)hashType));

			using(var sha256 = SHA256.Create())
				return sha256.ComputeHash(sha256.ComputeHash(verify.ToArray()));
		}

		public bool SigIsValid(byte[] pubKey, byte[] sigWithHashType, Script subScript, int inputIndex) {
			byte[] sig = ArrayHelpers.SubArray(sigWithHashType, 0, sigWithHashType.Length - 1);
			var hashType = (SigHash)sigWithHashType[sigWithHashType.Length - 1];

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

			var inputCount = data["vin_sz"].Value<ulong>();

			JToken[] inputs = data["in"].ToArray();
			for(ulong i = 0; i < inputCount; i++)
				Inputs.Add(Input.FromJson(inputs[i], this, (UInt32)i));

			var outputCount = data["vout_sz"].Value<ulong>();

			JToken[] outputs = data["out"].ToArray();
			for(ulong i = 0; i < outputCount; i++)
				Outputs.Add(Output.FromJson(outputs[i], this, (UInt32)i));
		}



		public override int GetHashCode() {
			return Hash.GetHashCode();
		}


		
		public byte[] ToByteArray() {
			using(var stream = new MemoryStream()) {
				using(var writer = new BinaryWriter(stream)) {
					WritePayload(writer);
					return stream.ToArray();
				}
			}
		}

		public string ToHex() {
			return BinaryHelpers.ByteArrayToHex(ToByteArray());
		}

		public static Transaction FromByteArray(byte[] bytes) {
			using(var stream = new MemoryStream(bytes)) {
				using(var reader = new BinaryReader(stream)) {
					var tx = new Transaction();
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
			var tx = new Transaction();
			tx.ReadFromJson(data);
			return tx;
		}

		public static Transaction Create(List<Output> inpoints, Dictionary<Address, Money> destinations, IDictionary<Address, ECKey> privateKeys, bool allowDust = false) {
			if(!allowDust && destinations.Select(d => d.Value).Sum() <= DustValue)
				throw new DustException();

			var tx = new Transaction { Version = CurrentVersion };

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
				byte[] sig = tx.GenerateInputSignature(key, SigHash.All, input.Outpoint.ScriptPubKey, (int)input.Index);
				input.ScriptSig = Script.Create(sig, key.PubKey);
			}

			tx.LockTime = 0U;

			return tx;
		}
	}
}

