using System;
using System.IO;

namespace Electrolyte.Messages {
	public abstract class Message : IBinary {
		protected MessageHeader _header;
		public MessageHeader Header {
			get { return _header; }
			set {
				if(!CommandIsValid(value.Command))
					_header = value;
			}
		}

		protected Message(BinaryReader reader) {
			Header = MessageHeader.Read(reader);
		}

		public virtual bool CommandIsValid(string command) { return true; }

		public abstract void Write(BinaryWriter writer);
	}
}

