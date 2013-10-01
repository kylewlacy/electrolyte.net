using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Tiko;
using AbstractTcpStream = Electrolyte.Portable.Net.TcpStream;
using InternalTcpClient = System.Net.Sockets.TcpClient;

namespace Electrolyte.Standard.Net {
	[Resolves(typeof(AbstractTcpStream))]
	public class TcpStream : AbstractTcpStream {
		protected InternalTcpClient InternalTcpClient;

		protected Stream InternalStream {
			get { return InternalTcpClient.GetStream(); }
		}

		public override bool CanRead {
			get { return InternalStream.CanRead; }
		}

		public override bool CanWrite {
			get { return InternalStream.CanWrite; }
		}

		public override bool CanSeek {
			get { return InternalStream.CanSeek; }
		}

		public override bool CanTimeout {
			get { return InternalStream.CanTimeout; }
		}

		public override long Length {
			get { return InternalStream.Length; }
		}

		public override long Position {
			get { return InternalStream.Position; }
			set { InternalStream.Position = value; }
		}

		public override int ReadTimeout {
			get { return InternalStream.ReadTimeout; }
			set { InternalStream.ReadTimeout = value; }
		}

		public override int WriteTimeout {
			get { return InternalStream.WriteTimeout; }
			set { InternalStream.WriteTimeout = value; }
		}

		protected override void Initialize(string server = null, int port = 80) {
			Server = server;
			Port = port;
			InternalTcpClient = new InternalTcpClient();
		}



		public override int Read(byte[] buffer, int offset, int count) {
			return InternalStream.Read(buffer, offset, count);
		}

		public override void Write(byte[] buffer, int offset, int count) {
			InternalStream.Write(buffer, offset, count);
		}

		public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
			return InternalStream.ReadAsync(buffer, offset, count);
		}

		public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
			return InternalStream.WriteAsync(buffer, offset, count);
		}

		public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state) {
			return InternalStream.BeginRead(buffer, offset, count, callback, state);
		}

		public override int EndRead(IAsyncResult asyncResult) {
			return InternalStream.EndRead(asyncResult);
		}

		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state) {
			return InternalStream.BeginWrite(buffer, offset, count, callback, state);
		}

		public override void EndWrite(IAsyncResult asyncResult) {
			InternalStream.EndWrite(asyncResult);
		}

		public override int ReadByte() {
			return InternalStream.ReadByte();
		}

		public override void WriteByte(byte value) {
			InternalStream.WriteByte(value);
		}

		public override void Flush() {
			InternalStream.Flush();
		}

		public override Task FlushAsync(CancellationToken cancellationToken) {
			return InternalStream.FlushAsync(cancellationToken);
		}

		public override Task CopyToAsync(System.IO.Stream destination, int bufferSize, CancellationToken cancellationToken) {
			return InternalStream.CopyToAsync(destination, bufferSize);
		}

		public override long Seek(long offset, System.IO.SeekOrigin origin) {
			return InternalStream.Seek(offset, origin);
		}

		public override void SetLength(long value) {
			InternalStream.SetLength(value);
		}

		public override void Close() {
			InternalStream.Close();
		}



		public override void Connect() {
			InternalTcpClient.Connect(Server, Port);
		}
	}
}

