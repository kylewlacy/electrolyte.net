using System;
using System.IO;
using System.Net;
using System.Text;
using Electrolyte.Helpers;

namespace Electrolyte {
	public class VersionMessage : Message {
		// https://en.bitcoin.it/wiki/Protocol_specification#version
		[Flags]
		public enum Services : ulong {
			None = 0,
			NodeNetwork = 1
		}

		protected override string Command {
			get { return "version"; }
		}

		public Int32 Version;
		public Services AvailableServices;
		public DateTime Time;
		public IPAddress Sender, Receiver;
		public UInt64 Nonce;
		public string UserAgent;
		public Int32 BlockHeight;
		public bool Relay;

		public VersionMessage(BinaryReader reader) : base(reader) {
			Version = reader.ReadInt32();
			AvailableServices = (Services)reader.ReadUInt64();

			Time = UnixTimeHelpers.DateTimeFromUnixTime(reader.ReadInt64());


			reader.ReadBytes(26); // Receiver
			reader.ReadBytes(26); // Sender

			Nonce = reader.ReadUInt64();
			UserAgent = VarString.FromBinaryReader(reader).Value;
			BlockHeight = reader.ReadInt32();

			Relay = (Version >= 70001) ? reader.ReadBoolean() : false; // BIP 0037
		}
	}
}

