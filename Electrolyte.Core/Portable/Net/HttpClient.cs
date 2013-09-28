using System;
using System.Threading.Tasks;
using Tiko;

namespace Electrolyte.Portable.Net {
	public abstract class HttpClient {
		public abstract void Connect();
		public abstract void Disconnect();
		public abstract Task<string> GetAsync(Uri uri);
		public abstract Task<string> PostAsync(Uri uri, string content);

		protected abstract void Initialize();

		public static HttpClient Create() {
			var httpClient = TikoContainer.Resolve<HttpClient>();
			httpClient.Initialize();
			return httpClient;
		}
	}
}

