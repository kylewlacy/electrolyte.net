using System;
using System.IO;

namespace Electrolyte.Portable.Net {
	public abstract class TcpStream : Stream {
		public abstract string Server { get; set; }
		public abstract int Port { get; set; }

		public TcpStream() { }

		protected abstract void Initialize();

		public abstract void Connect();
		public virtual void Connect(string server, int port) {
			Server = server;
			Port = port;
			Connect();
		}

		public static TcpStream Create(string server = null, int port = 80) {
			throw new NotImplementedException();
		}
	}
}

