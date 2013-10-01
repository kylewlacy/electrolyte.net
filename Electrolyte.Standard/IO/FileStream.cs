using System;
using System.Threading;
using System.Threading.Tasks;
using Tiko;
using Electrolyte.Portable.IO;
using AbstractFileStream = Electrolyte.Portable.IO.FileStream;
using InternalFileStream = System.IO.FileStream;

namespace Electrolyte.Standard.IO {
	[Resolves(typeof(AbstractFileStream))]
	public class FileStream : AbstractFileStream {
		protected InternalFileStream InternalFileStream;

		public override bool CanRead {
			get { return InternalFileStream.CanRead; }
		}

		public override bool CanWrite {
			get { return InternalFileStream.CanWrite; }
		}

		public override bool CanSeek {
			get { return InternalFileStream.CanSeek; }
		}

		public override bool CanTimeout {
			get { return InternalFileStream.CanTimeout; }
		}

		public override long Length {
			get { return InternalFileStream.Length; }
		}

		public override long Position {
			get { return InternalFileStream.Position; }
			set { InternalFileStream.Position = value; }
		}

		public override int ReadTimeout {
			get { return InternalFileStream.ReadTimeout; }
			set { InternalFileStream.ReadTimeout = value; }
		}

		public override int WriteTimeout {
			get { return InternalFileStream.WriteTimeout; }
			set { InternalFileStream.WriteTimeout = value; }
		}

		
		protected override void Initialize(Electrolyte.Portable.IO.FileInfo info, FileMode mode) {
			File = info;
			Mode = mode;

			switch(mode) {
			case FileMode.Append:
			case FileMode.Create:
			case FileMode.CreateNew:
			case FileMode.OpenOrCreate:
			case FileMode.Truncate:
				if(!info.Location.Exists)
					info.Location.Create();
				break;
			}

			InternalFileStream = new InternalFileStream(info.Path, ToInternalFileMode(mode));
		}

		public override int Read(byte[] buffer, int offset, int count) {
			return InternalFileStream.Read(buffer, offset, count);
		}

		public override void Write(byte[] buffer, int offset, int count) {
			InternalFileStream.Write(buffer, offset, count);
		}

		public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
			return InternalFileStream.ReadAsync(buffer, offset, count);
		}

		public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) {
			return InternalFileStream.WriteAsync(buffer, offset, count);
		}

		public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state) {
			return InternalFileStream.BeginRead(buffer, offset, count, callback, state);
		}

		public override int EndRead(IAsyncResult asyncResult) {
			return InternalFileStream.EndRead(asyncResult);
		}

		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state) {
			return InternalFileStream.BeginWrite(buffer, offset, count, callback, state);
		}

		public override void EndWrite(IAsyncResult asyncResult) {
			InternalFileStream.EndWrite(asyncResult);
		}

		public override int ReadByte() {
			return InternalFileStream.ReadByte();
		}

		public override void WriteByte(byte value) {
			InternalFileStream.WriteByte(value);
		}

		public override void Flush() {
			InternalFileStream.Flush();
		}

		public override Task FlushAsync(CancellationToken cancellationToken) {
			return InternalFileStream.FlushAsync(cancellationToken);
		}

		public override Task CopyToAsync(System.IO.Stream destination, int bufferSize, CancellationToken cancellationToken) {
			return InternalFileStream.CopyToAsync(destination, bufferSize);
		}

		public override long Seek(long offset, System.IO.SeekOrigin origin) {
			return InternalFileStream.Seek(offset, origin);
		}

		public override void SetLength(long value) {
			InternalFileStream.SetLength(value);
		}

		public override void Close() {
			InternalFileStream.Close();
		}



		protected static System.IO.FileMode ToInternalFileMode(FileMode fileMode) {
			return (System.IO.FileMode)((int)fileMode);
		}
	}
}

