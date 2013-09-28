using System;
using Org.BouncyCastle.Crypto.Tls;
using Electrolyte.Portable.Networking;

namespace Electrolyte.Networking {
	public class SecureElectrumProtocol : ElectrumProtocol {
		public SecureElectrumProtocol(string server, int port) : base(server, port) { }

		public override void Connect() {
			base.Connect();

			var sslStream = new SslStream(Client.GetStream());
			sslStream.CertificateIsValid += ValidateCertificate;
			sslStream.Connect();

			ClientStream = sslStream;
		}

		static bool ValidateCertificate(Certificate certificate) {
			// TODO: SET UP PROPER VALIDATION
			// BAD BAD BAD BAD BAD
			return true;
		}
	}
}

