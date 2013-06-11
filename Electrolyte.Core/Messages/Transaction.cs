using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Electrolyte.Primitives;
using Electrolyte.Extensions;

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

		public override bool CommandIsValid(string command) {
			return command == "tx";
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

		protected override void WritePayload(BinaryWriter writer) {
			throw new NotImplementedException();
		}
	}
}

