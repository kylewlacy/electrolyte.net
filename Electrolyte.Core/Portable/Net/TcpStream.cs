using System;
using System.IO;
using Tiko;

namespace Electrolyte.Portable.Net {
	public abstract class TcpStream : Stream {
		public virtual string Server { get; set; }
		public virtual int Port { get; set; }

		public TcpStream() { }

		protected abstract void Initialize(string server = null, int port = 80);

		public abstract void Connect();
		public virtual void Connect(string server, int port) {
			Server = server;
			Port = port;
			Connect();
		}

		public static TcpStream Create(string server = null, int port = 80) {
			var tcpStream = TikoContainer.Resolve<TcpStream>();
			tcpStream.Initialize(server, port);
			return tcpStream;
		}
	}
}

