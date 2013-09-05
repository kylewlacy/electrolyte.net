using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;
using Electrolyte;
using Electrolyte.Extensions;
using Electrolyte.Primitives;
using Electrolyte.Helpers;

namespace Electrolyte.Messages {
	public partial class Transaction : Message<Transaction> {
		public struct Info {
			public string Hash;
			public ulong Height;

			public Info(string hash, ulong height) {
				Hash = hash;
				Height = height;
			}
		}
	}
}


