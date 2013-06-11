using System;
using System.IO;

namespace Electrolyte.Messages {
	public abstract class Message<T> where T : Message<T>, new() {
		protected MessageHeader _header;
		public MessageHeader Header {
			get { return _header; }
			set {
				if(!CommandIsValid(value.Command))
					_header = value;
			}
		}

		protected Message() { }

		public virtual bool CommandIsValid(string command) { return true; }

		public static T Read(BinaryReader reader) {
			T t = new T();

			t.Header = new MessageHeader();
			t.Header.ReadPayload(reader);

			t.ReadPayload(reader);

			return t;
		}

		public void Write(BinaryWriter writer) {
			Header.WritePayload(writer);
			WritePayload(writer);
		}

		protected abstract void ReadPayload(BinaryReader reader);

		protected abstract void WritePayload(BinaryWriter writer);
	}
}

