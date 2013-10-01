using System;
using System.Threading.Tasks;
using Tiko;
using AbstractHttpClient = Electrolyte.Portable.Net.HttpClient;
using InternalWebClient = System.Net.WebClient;

namespace Electrolyte.Standard.Net {
	[Resolves(typeof(AbstractHttpClient))]
	public class HttpClient : AbstractHttpClient {
		protected InternalWebClient InternalWebClient;

		protected override void Initialize() { }

		public void Connect(InternalWebClient webClient) {
			InternalWebClient = webClient;
		}

		public override void Connect() {
			Connect(new InternalWebClient());
		}

		public override void Disconnect() {
			InternalWebClient.Dispose();
		}

		public override Task<string> GetAsync(Uri uri) {
			return InternalWebClient.DownloadStringTaskAsync(uri);
		}

		public override Task<string> PostAsync(Uri uri, string content) {
			return InternalWebClient.UploadStringTaskAsync(uri, content);
		}
	}
}

