using System;
using System.IO;

namespace Electrolyte.Messages {
	public abstract class Message {
		protected MessageHeader _header;
		public MessageHeader Header {
			get { return _header; }
			set {
				if(!CommandIsValid(value.Command))
					_header = value;
			}
		}

		protected Message(MessageHeader header) {
			Header = header;
		}

		protected Message(BinaryReader reader) : this(new MessageHeader(reader)) { }

		public virtual bool CommandIsValid(string command) { return true; }
	}
}

