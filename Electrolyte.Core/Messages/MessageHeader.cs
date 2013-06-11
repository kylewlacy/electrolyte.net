using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Electrolyte.Messages {
	public class InvalidHeaderException : Exception { }

	public class MessageHeader : Message<MessageHeader> {
		public static long HeaderSize = 24;
		static byte[] MagicBytes = {0xF9, 0xBE, 0xB4, 0xD9};

		string _command;
		public string Command {
			get { return _command; }
			set { _command = value.Trim(new char[] { '\0' }); }
		}
		public UInt32 PayloadLength;
		public byte[] ExpectedChecksum;

		public long ExpectedMessageSize {
			get { return PayloadLength + HeaderSize; }
		}

		public MessageHeader() { }

		public MessageHeader(string command, UInt32 payloadLength, byte[] checksum) {
			Command = command;
			PayloadLength = payloadLength;
			ExpectedChecksum = checksum.Take(4).ToArray();
		}

		new public static MessageHeader Read(BinaryReader reader) {
			MessageHeader header = new MessageHeader();
			header.ReadPayload(reader);
			return header;
		}

		new public void Write(BinaryWriter writer) {
			WritePayload(writer);
		}

		protected override void ReadPayload(BinaryReader reader) {
			if(!reader.ReadBytes(4).SequenceEqual(MagicBytes) || reader.BaseStream.Length < HeaderSize)
				throw new InvalidHeaderException();
			Command = new String(reader.ReadChars(12));
			PayloadLength = reader.ReadUInt32();
			ExpectedChecksum = reader.ReadBytes(4);
		}

		protected override void WritePayload(BinaryWriter writer) {
			writer.Write(MagicBytes);

			byte[] command = Encoding.ASCII.GetBytes(Command);
			Array.Resize(ref command, 12);
			writer.Write(command);

			writer.Write(PayloadLength);
			writer.Write(ExpectedChecksum);
		}
	}
}

