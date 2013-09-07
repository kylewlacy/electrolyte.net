using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Electrolyte.Networking {
	public class SecureElectrumProtocol : ElectrumProtocol {
		static X509Certificate serverCertificate;

		public SecureElectrumProtocol(string server, int port) : base(server, port) { }



		public override void Connect() {
			base.Connect();

			SslStream sslStream = new SslStream(Client.GetStream(), false, (sender, certificate, chain, sslPolicyErrors) => {
				Console.WriteLine(sslPolicyErrors);
				foreach(var s in chain.ChainStatus)
					Console.WriteLine(s.Status);

				// TODO: SET UP PROPER VALIDATION
				// BAD BAD BAD BAD BAD
				return true;
			});

			sslStream.AuthenticateAsClient(Server);
			ClientStream = sslStream;
		}
	}
}

