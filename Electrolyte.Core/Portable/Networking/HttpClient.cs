using System;
using System.Threading.Tasks;

namespace Electrolyte.Portable.Networking {
	public abstract class HttpClient {
		public abstract void Connect();
		public abstract void Disconnect();
		public abstract Task<string> GetAsync(Uri uri);
		public abstract Task<string> PostAsync(Uri uri, string content);

		public static HttpClient Create() {
			throw new NotImplementedException();
		}
	}
}

