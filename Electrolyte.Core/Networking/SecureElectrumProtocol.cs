using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Electrolyte.Networking {
	public class SecureElectrumProtocol : ElectrumProtocol {
		public SecureElectrumProtocol(string server, int port) : base(server, port) { }

		public override void Connect() {
			base.Connect();

			var sslStream = new SslStream(Client.GetStream(), false, new RemoteCertificateValidationCallback(ValidateCertificate));

			sslStream.AuthenticateAsClient(Server);
			ClientStream = sslStream;
		}

		static bool ValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors policyErrors) {
			// TODO: SET UP PROPER VALIDATION
			// BAD BAD BAD BAD BAD
			return true;
		}
	}
}

