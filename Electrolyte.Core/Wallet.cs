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

		public byte[] EncryptedData;
		public byte[] IV, Salt;

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

		public Wallet() {
			PrivateKeys = new Dictionary<string, byte[]>();
			WatchAddresses = new List<string>();
		}

		public Wallet(Dictionary<string, byte[]> keys) {
			PrivateKeys = keys;
			WatchAddresses = new List<string>();
		}



		public void GenerateKey() {
			ImportKey(new ECKey());
		}

		public void ImportKey(string key) {
			ImportKey(ECKey.FromWalletImportFormat(key));
		}

		void ImportKey(ECKey key) {
			PrivateKeys.Add(key.ToAddress().ToString(), key.PrivateKeyBytes);
		}



		public void LoadFromJson(string json) {
			JObject data = JObject.Parse(json);
			if(data["version"].Value<ulong>() != Version)
				throw new FormatException(String.Format("Invalid wallet version: {0}", data["version"].Value<ulong>()));

			IV = Base58.DecodeWithChecksum(data["encrypted"]["iv"].Value<string>());
			Salt = Base58.DecodeWithChecksum(data["encrypted"]["salt"].Value<string>());

			EncryptedData = Base58.DecodeWithChecksum(data["encrypted"]["data"].Value<string>());
		}

		void LoadPrivateDataFromJson(string json) {
			JObject data = JObject.Parse(json);
			foreach(JToken key in data["keys"]) {
				PrivateKeys.Add(key["addr"].Value<string>(), ECKey.FromWalletImportFormat(key["priv"].Value<string>()).PrivateKeyBytes);
			}
		}

		public void Read(TextReader reader) {
			LoadFromJson(reader.ReadToEnd());
		}

		public void ReadPrivate(TextReader reader) {
			LoadPrivateDataFromJson(reader.ReadToEnd());
		}

		public void Decrypt(string passphrase) {
			Aes aes = Aes.Create();
			aes.Mode = CipherMode.CBC;
			aes.IV = IV;
			aes.Key = PassphraseToKey(passphrase, Salt);

			using(MemoryStream stream = new MemoryStream(EncryptedData)) {
				using(CryptoStream cryptoStream = new CryptoStream(stream, aes.CreateDecryptor(), CryptoStreamMode.Read))
					using(StreamReader streamReader = new StreamReader(cryptoStream))
						ReadPrivate(streamReader);
			}
		}

		public void Decrypt(TextReader reader, string passphrase) {
			Read(reader);
			Decrypt(passphrase);
		}

		public string DataAsJson() {
			Dictionary<string, object> data = new Dictionary<string, object> {
				{ "version", Version },
				{ "watch_addresses", new List<object>() },
				{ "encrypted", new Dictionary<string, object> {
						{ "iv", Base58.EncodeWithChecksum(IV) },
						{ "salt", Base58.EncodeWithChecksum(Salt) },
						{ "data", Base58.EncodeWithChecksum(EncryptedData) }
				} }
			};

			foreach(string address in WatchAddresses) {
				((List<object>)data["watch_addresses"]).Add(new Dictionary<string,object> {
					{ "addr", address }
				});
			}

			return JsonConvert.SerializeObject(data);
		}

		string PrivateDataAsJson() {
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

		public void Write(TextWriter writer) {
			writer.Write(DataAsJson());
		}

		public void WritePrivate(TextWriter writer) {
			writer.Write(PrivateDataAsJson());
		}

		public void Encrypt(string passphrase) {
			SecureRandom random = new SecureRandom();

			IV = new byte[16];
			random.NextBytes(IV);

			Salt = new byte[128];
			random.NextBytes(Salt);

			Aes aes = Aes.Create();
			aes.Mode = CipherMode.CBC;
			aes.IV = IV;
			aes.Key = PassphraseToKey(passphrase, Salt);

			using(MemoryStream stream = new MemoryStream()) {
				using(CryptoStream cryptoStream = new CryptoStream(stream, aes.CreateEncryptor(), CryptoStreamMode.Write))
					using(StreamWriter streamWriter = new StreamWriter(cryptoStream))
						WritePrivate(streamWriter);
				EncryptedData = stream.ToArray();
			}
		}

		public void Encrypt(TextWriter writer, string passphrase) {
			Encrypt(passphrase);
			Write(writer);
		}

		static byte[] PassphraseToKey(byte[] passphrase, byte[] salt) {
			byte[] key = passphrase;

			using(SHA256 sha256 = SHA256.Create()) {
				for(ulong i = 0; i < KeyHashes; i++) {
					key = sha256.ComputeHash(ArrayHelpers.ConcatArrays(key, passphrase, salt));
				}
			}

			return ArrayHelpers.SubArray(key, 0, 32);
		}

		static byte[] PassphraseToKey(string passphrase, byte[] salt) {
			return PassphraseToKey(Encoding.UTF8.GetBytes(passphrase), salt);
		}
	}
}

