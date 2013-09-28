using System;
using Electrolyte.Portable;
using NUnit.Framework;

namespace Electrolyte.Test {
	[TestFixture]
	public class ECKeyTest {
		Tuple<string, byte[], string>[] Keys = new Tuple<string, byte[], string>[] {
			Tuple.Create(
				"5Kb8kLf9zgWQnogidDA76MzPL6TsZZY36hWXMssSzNydYXYB9KF",
				new byte[] {0xE9, 0x87, 0x3D, 0x79, 0xC6, 0xD8, 0x7D, 0xC0, 0xFB, 0x6A, 0x57, 0x78, 0x63, 0x33, 0x89, 0xF4, 0x45, 0x32, 0x13, 0x30, 0x3D, 0xA6, 0x1F, 0x20, 0xBD, 0x67, 0xFC, 0x23, 0x3A, 0xA3, 0x32, 0x62 },
				"1CC3X2gu58d6wXUWMffpuzN9JAfTUWu4Kj"
			)
		};

		[Test]
		public void WIFToKey() {
			foreach(Tuple<string, byte[], string> key in Keys) {
				Assert.AreEqual(key.Item2, ECKey.FromWalletImportFormat(key.Item1).PrivateKeyBytes);
			}
		}

		[Test]
		public void KeyToWIF() {
			foreach(Tuple<string, byte[], string>key in Keys) {
				Assert.AreEqual(key.Item1, new ECKey(key.Item2).ToWalletImportFormat());
			}
		}

		[Test]
		public void KeyToAddress() {
			foreach(Tuple<string, byte[], string>key in Keys) {
				Assert.AreEqual(key.Item3, new ECKey(key.Item2).ToAddress().ToString());
			}
		}
	}
}

