using System;
using System.IO;
using System.Net;
using System.Linq;
using Electrolyte.Helpers;
using Electrolyte.Primitives;

namespace Electrolyte.Messages {
	public class VersionMessage : Message {
		// https://en.bitcoin.it/wiki/Protocol_specification#version
		[Flags]
		public enum Services : ulong {
			None = 0,
			NodeNetwork = 1
		}

		public class NetworkNode {
			public Services AvailableServices;
			public IPAddress Address;
			public UInt16 Port;
			static byte[] IPv4Wrapper = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xFF, 0xFF };

			public NetworkNode(UInt64 services, byte[] address, UInt16 port) {
				AvailableServices = (Services)services;

				if(address.Take(IPv4Wrapper.Length).SequenceEqual(IPv4Wrapper))
					address = address.Reverse().Take(address.Length - IPv4Wrapper.Length).Reverse().ToArray();
				Address = new IPAddress(address);

				Port = port;
			}

			public static NetworkNode FromBinaryReader(BinaryReader reader) {
				UInt64 availableServices = reader.ReadUInt64();
				byte[] address = reader.ReadBytes(16);
				UInt16 port = BitConverter.ToUInt16(reader.ReadBytes(2).Reverse().ToArray(), 0); // TODO: Big-endian extension/helper methods
				return new NetworkNode(availableServices, address, port);
			}
		}

		protected override string Command {
			get { return "version"; }
		}

		public Int32 Version;
		public Services AvailableServices;
		public DateTime Time;
		public NetworkNode Sender, Receiver;
		public UInt64 Nonce;
		public string UserAgent;
		public Int32 BlockHeight;
		public bool Relay;

		public VersionMessage(BinaryReader reader) : base(reader) {
			Version = reader.ReadInt32();
			AvailableServices = (Services)reader.ReadUInt64();

			Time = UnixTimeHelpers.DateTimeFromUnixTime(reader.ReadInt64());

			Receiver = NetworkNode.FromBinaryReader(reader);
			Sender = NetworkNode.FromBinaryReader(reader);

			Nonce = reader.ReadUInt64();
			UserAgent = VarString.FromBinaryReader(reader).Value;
			BlockHeight = reader.ReadInt32();

			Relay = (Version >= 70001) ? reader.ReadBoolean() : false; // BIP 0037
		}
	}
}

