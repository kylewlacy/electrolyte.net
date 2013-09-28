using System;
using System.IO;
using Org.BouncyCastle.Crypto.Tls;
using Electrolyte.Portable.Net;

namespace Electrolyte.Networking {
	public class SecureElectrumProtocol : ElectrumProtocol {
		public SecureElectrumProtocol(string server, int port) : base(server, port) { }

		protected SslStream SslStream;
		protected override Stream ClientStream {
			get { return SslStream; }
		}

		public override void Connect() {
			base.Connect();

			SslStream = new SslStream(Client);
			SslStream.CertificateIsValid += ValidateCertificate;
			SslStream.Connect();
		}

		static bool ValidateCertificate(Certificate certificate) {
			// TODO: SET UP PROPER VALIDATION
			// BAD BAD BAD BAD BAD
			return true;
		}
	}
}

