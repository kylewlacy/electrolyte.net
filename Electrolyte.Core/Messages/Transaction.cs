using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using Org.BouncyCastle;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Electrolyte;
using Electrolyte.Primitives;
using Electrolyte.Extensions;
using Electrolyte.Helpers;

namespace Electrolyte.Messages {
	public class Transaction : Message<Transaction> {
		public static UInt32 CurrentVersion = 1;

		public class Input {
			//public Script ScriptSig;
			public Output Output;
			public UInt32 Sequence;

			public Input(BinaryReader reader) {
				reader.ReadBytes(36); 											// Outpoint

				UInt64 scriptLength = VarInt.Read(reader).Value;	// Script length
				reader.ReadBytes((int)scriptLength);							// Script (TODO: read bytes as many time as needed to read all)

				Sequence = reader.ReadUInt32();									// Sequence (unused)
			}
		}

		public class Output {
			//public Script ScriptPubKey;
			public Int64 Value;

			public Output(BinaryReader reader) {
				Value = reader.ReadInt64();

				UInt64 scriptLength = VarInt.Read(reader).Value;	// Script length
				reader.ReadBytes((int)scriptLength);							// Script (TODO: read bytes as many time as needed to read all)
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
				Inputs.Add(new Input(reader));
			}

			UInt64 outputCount = VarInt.Read(reader).Value;
			for(ulong i = 0; i < outputCount; i++) {
				Outputs.Add(new Output(reader));
			}

			LockTime = reader.ReadUInt32();
		}

		public override void WritePayload(BinaryWriter writer) {
			throw new NotImplementedException();
		}
	}
}

