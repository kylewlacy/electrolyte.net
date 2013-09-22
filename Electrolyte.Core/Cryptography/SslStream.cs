using System;
using System.IO;
using Org.BouncyCastle.Crypto.Tls;

namespace Electrolyte.Cryptography {
	public class SslStream : Stream {
		public class InvalidCertificateException : TlsException {
			public InvalidCertificateException() : base() { }
			public InvalidCertificateException(string message) : base(message) { }
			public InvalidCertificateException(string message, Exception e) : base(message, e) { }
		}

		public delegate bool ValidateCertificate(Certificate certificate);
		public event ValidateCertificate CertificateIsValid = delegate { return true; };

		class TlsClient : DefaultTlsClient {
			readonly SslStream SslStream;

			public TlsClient(SslStream sslStream) {
				SslStream = sslStream;
			}

			public override TlsAuthentication GetAuthentication() {
				return new SslAuthentication(SslStream);
			}
		}

		class SslAuthentication : TlsAuthentication {
			readonly SslStream SslStream;

			public SslAuthentication(SslStream sslStream) {
				SslStream = sslStream;
			}

			public TlsCredentials GetClientCredentials(CertificateRequest request) {
				return null;
			}

			public void NotifyServerCertificate(Certificate certificate) {
				bool valid = true;

				foreach(ValidateCertificate del in SslStream.CertificateIsValid.GetInvocationList())
					valid &= del(certificate);

				if(!valid)
					throw new InvalidCertificateException();
			}
		}

		TlsProtocolHandler handler;

		Stream stream {
			get { return handler.Stream; }
		}

		public SslStream(Stream stream) {
			handler = new TlsProtocolHandler(stream);
		}

		public void Connect() {
			handler.Connect(new TlsClient(this));
		}



		public override bool CanRead {
			get { return stream.CanRead; }
		}

		public override bool CanSeek {
			get { return stream.CanSeek; }
		}

		public override bool CanTimeout {
			get { return stream.CanTimeout; }
		}

		public override bool CanWrite {
			get { return stream.CanWrite; }
		}

		public override long Length {
			get { return stream.Length; }
		}

		public override long Position {
			get { return stream.Position; }
			set { stream.Position = value; }
		}

		public override int ReadTimeout {
			get { return stream.ReadTimeout; }
			set { stream.ReadTimeout = value; }
		}

		public override int WriteTimeout {
			get { return stream.WriteTimeout; }
			set { stream.WriteTimeout = value; }
		}

		public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state) {
			return stream.BeginRead(buffer, offset, count, callback, state);
		}

		public override int EndRead(IAsyncResult asyncResult) {
			return stream.EndRead(asyncResult);
		}

		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state) {
			return stream.BeginWrite(buffer, offset, count, callback, state);
		}

		public override void EndWrite(IAsyncResult asyncResult) {
			stream.EndWrite(asyncResult);
		}

		public override int Read(byte[] buffer, int offset, int count) {
			return stream.Read(buffer, offset, count);
		}

		public override void Write(byte[] buffer, int offset, int count) {
			stream.Write(buffer, offset, count);
		}

		public override int ReadByte() {
			return stream.ReadByte();
		}

		public override void WriteByte(byte value) {
			stream.WriteByte(value);
		}

		public override long Seek(long offset, SeekOrigin origin) {
			return stream.Seek(offset, origin);
		}

		public override void SetLength(long value) {
			stream.SetLength(value);
		}

		protected override void Dispose(bool disposing) {
			stream.Dispose();
		}

		public override void Flush() {
			stream.Flush();
		}
	}
}

