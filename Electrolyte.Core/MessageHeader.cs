using System;
using System.IO;
using System.Linq;

namespace Electrolyte {
	public class InvalidHeaderException : Exception { }

	public class MessageHeader {
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

		public MessageHeader(BinaryReader reader) {
			if(!reader.ReadBytes(4).SequenceEqual(MagicBytes) || reader.BaseStream.Length < HeaderSize)
				throw new InvalidHeaderException();
			Command = new String(reader.ReadChars(12));
			PayloadLength = reader.ReadUInt32();
			ExpectedChecksum = reader.ReadBytes(4);
		}
	}
}

