using System;
using System.IO;

namespace Electrolyte {
	public abstract class Message {
		public MessageHeader Header { get; private set; }

		protected abstract string Command { get; }

		protected Message(BinaryReader reader) {
			Header = new MessageHeader(reader);
			if(Header.Command != Command)
				throw new InvalidHeaderException();
		}
	}
}

