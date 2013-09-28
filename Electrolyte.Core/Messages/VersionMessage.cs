using System;
using System.IO;
using Electrolyte.Helpers;
using Electrolyte.Portable;
using Electrolyte.Extensions;
using Electrolyte.Portable.Networking;

namespace Electrolyte.Messages {
	public class VersionMessage : Message<VersionMessage> {
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

			public NetworkNode(UInt64 services, byte[] address, UInt16 port) {
				AvailableServices = (Services)services;
				Address = new IPAddress(address).UnwrapFromIPv6();
				Port = port;
			}

			public NetworkNode(Services services, IPAddress address, UInt16 port) {
				AvailableServices = services;
				Address = address;
				Port = port;
			}

			public static NetworkNode FromBinaryReader(BinaryReader reader) {
				return new NetworkNode(reader.ReadUInt64(), reader.ReadBytes(16), reader.ReadUInt16(Endian.Big));
			}

			public void Write(BinaryWriter writer) {
				writer.Write((UInt64)AvailableServices);
				writer.Write(Address.WrapToIPv6().GetAddressBytes());
				writer.Write(Port, Endian.Big);

			}
		}

		public Int32 Version;
		public Services AvailableServices;
		public DateTime Time;
		public NetworkNode Sender, Recipient;
		public UInt64 Nonce;
		public string UserAgent;
		public Int32 BlockHeight;
		public bool Relay;

		public override string ExpectedCommand {
			get { return "version"; }
		}

		public VersionMessage() { }

		public VersionMessage(Int32 version, Services services, DateTime time, NetworkNode recipient, NetworkNode sender, UInt64 nonce, string userAgent, Int32 height) {
			Version = version;
			AvailableServices = services;
			Time = time;
			Sender = sender;
			Recipient = recipient;
			Nonce = nonce;
			UserAgent = userAgent;
			BlockHeight = height;
		}

		public override void ReadPayload(BinaryReader reader) {
			Version = reader.ReadInt32();
			AvailableServices = (Services)reader.ReadUInt64();

			Time = UnixTime.DateTimeFromUnixTime(reader.ReadInt64());

			Recipient = NetworkNode.FromBinaryReader(reader);
			Sender = NetworkNode.FromBinaryReader(reader);

			Nonce = reader.ReadUInt64();
			UserAgent = VarString.Read(reader).Value;
			BlockHeight = reader.ReadInt32();

			Relay = (Version >= 70001) && reader.ReadBoolean(); // BIP 0037
		}

		public override void WritePayload(BinaryWriter writer) {
			writer.Write(Version);
			writer.Write((UInt64)AvailableServices);

			writer.Write(UnixTime.UnixTimeFromDateTime(Time));

			Recipient.Write(writer);
			Sender.Write(writer);

			writer.Write(Nonce);
			new VarString(UserAgent).Write(writer);
			writer.Write(BlockHeight);

			if(Version >= 70001)
				writer.Write(Relay);
		}
	}
}

