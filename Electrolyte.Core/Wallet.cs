using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
using Org.BouncyCastle.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Electrolyte.Primitives;
using Electrolyte.Helpers;

namespace Electrolyte {
	public class Wallet {
		public static UInt64 Version = 1000;

#if !DEBUG
		public static ulong KeyHashes = 1048576;
#else
		public static ulong KeyHashes = 1024;
#endif

		public Dictionary<string, byte[]> PrivateKeys;
		public List<string> WatchAddresses;
		public List<string> Addresses {
			get {
				List<string> addresses = new List<string>(PrivateKeys.Keys);
				foreach(string address in WatchAddresses) {
					if(!addresses.Contains(address))
						addresses.Add(address);
				}
				return addresses;
			}
		}

		public void Decrypt(TextReader reader, string passphrase) {
			JObject data = JObject.Parse(reader.ReadToEnd());
			if(data["version"].Value<ulong>() != Version)
				throw new FormatException(String.Format("Invalid wallet version: {0}", data["version"].Value<ulong>()));

			byte[] iv = Base58.DecodeWithChecksum(data["iv"].Value<string>());
			byte[] salt = Base58.DecodeWithChecksum(data["salt"].Value<string>());

			Aes aes = Aes.Create();
			aes.Mode = CipherMode.CBC;
			aes.IV = iv;
			aes.Key = PassphraseToKey(passphrase, salt);

			using(MemoryStream stream = new MemoryStream(Base58.DecodeWithChecksum(data["encrypted"].Value<string>()))) {
				using(CryptoStream cryptoStream = new CryptoStream(stream, aes.CreateDecryptor(), CryptoStreamMode.Read))
					using(StreamReader streamReader = new StreamReader(cryptoStream))
						ReadPrivate(streamReader);
			}
		}

		public void ReadPrivate(TextReader reader) {
			JObject data = JObject.Parse(reader.ReadToEnd());
			foreach(JToken key in data["keys"]) {
				PrivateKeys.Add(key["addr"].Value<string>(), ECKey.FromWalletImportFormat(key["priv"].Value<string>()).PrivateKeyBytes);
			}
		}

		public Wallet() {
			PrivateKeys = new Dictionary<string, byte[]>();
			WatchAddresses = new List<string>();
		}

		public Wallet(Dictionary<string, byte[]> keys) {
			PrivateKeys = keys;
			WatchAddresses = new List<string>();
		}

		public void Encrypt(TextWriter writer, string passphrase, SecureRandom random) {
			byte[] iv = new byte[16];
			random.NextBytes(iv);

			byte[] salt = new byte[128];

			Dictionary<string, object> data = new Dictionary<string, object> {
				{ "version", Version },
				{ "iv", Base58.EncodeWithChecksum(iv) },
				{ "salt", Base58.EncodeWithChecksum(salt) },
				{ "watch_addresses", new List<object>() }
			};

			foreach(string address in WatchAddresses) {
				((List<object>)data["watch_addresses"]).Add(new Dictionary<string,object> {
					{ "addr", address }
				});
			}

			Aes aes = Aes.Create();
			aes.Mode = CipherMode.CBC;
			aes.IV = iv;
			aes.Key = PassphraseToKey(passphrase, salt);

			using(MemoryStream stream = new MemoryStream()) {
				using(CryptoStream cryptoStream = new CryptoStream(stream, aes.CreateEncryptor(), CryptoStreamMode.Write))
					using(StreamWriter streamWriter = new StreamWriter(cryptoStream))
						WritePrivate(streamWriter);

				data.Add("encrypted", Base58.EncodeWithChecksum(stream.ToArray()));
			}

			writer.Write(JsonConvert.SerializeObject(data));
		}

		public void Encrypt(TextWriter writer, string passphrase) {
			Encrypt(writer, passphrase, new SecureRandom());
		}

		public void WritePrivate(TextWriter writer) {
			writer.Write(SecureDataAsJson());
		}

		public string SecureDataAsJson() {
			Dictionary<string, object> data = new Dictionary<string, object>();
			data.Add("keys", new List<object>());
			foreach(KeyValuePair<string, byte[]> privateKey in PrivateKeys) {
				((List<object>)data["keys"]).Add(new Dictionary<string, object> {
					{ "addr", privateKey.Key },
					{ "priv", new ECKey(privateKey.Value).ToWalletImportFormat() }
				});
			}

			return JsonConvert.SerializeObject(data);
		}

		public void GenerateKey() {
			ImportKey(new ECKey());
		}

		public void ImportKey(string key) {
			ImportKey(ECKey.FromWalletImportFormat(key));
		}

		public void ImportKey(ECKey key) {
			PrivateKeys.Add(key.ToAddress().ToString(), key.PrivateKeyBytes);
		}

		public static byte[] PassphraseToKey(byte[] passphrase, byte[] salt) {
			byte[] key = passphrase;

			using(SHA256 sha256 = SHA256.Create()) {
				for(ulong i = 0; i < KeyHashes; i++) {
					key = sha256.ComputeHash(ArrayHelpers.ConcatArrays(key, passphrase, salt));
				}
			}

			return ArrayHelpers.SubArray(key, 0, 32);
		}

		public static byte[] PassphraseToKey(string passphrase, byte[] salt) {
			return PassphraseToKey(Encoding.UTF8.GetBytes(passphrase), salt);
		}
	}
}

