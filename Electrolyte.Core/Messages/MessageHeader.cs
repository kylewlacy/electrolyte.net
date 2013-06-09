using System;
using System.IO;
using System.Linq;
using System.Text;

namespace Electrolyte.Messages {
	public class InvalidHeaderException : Exception { }

	public class MessageHeader : IBinary {
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

		public MessageHeader(string command, UInt32 payloadLength, byte[] checksum) {
			Command = command;
			PayloadLength = payloadLength;
			ExpectedChecksum = checksum.Take(4).ToArray();
		}

		MessageHeader(BinaryReader reader) {
			if(!reader.ReadBytes(4).SequenceEqual(MagicBytes) || reader.BaseStream.Length < HeaderSize)
				throw new InvalidHeaderException();
			Command = new String(reader.ReadChars(12));
			PayloadLength = reader.ReadUInt32();
			ExpectedChecksum = reader.ReadBytes(4);
		}

		public static MessageHeader Read(BinaryReader reader) {
			return new MessageHeader(reader);
		}

		public void Write(BinaryWriter writer) {
			writer.Write(MagicBytes);

			byte[] command = Encoding.ASCII.GetBytes(Command);
			Array.Resize(ref command, 12);
			writer.Write(command);

			writer.Write(PayloadLength);
			writer.Write(ExpectedChecksum);
		}
	}
}

