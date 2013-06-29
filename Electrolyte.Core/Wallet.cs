using System;
using System.IO;
using System.Text;
using System.Timers;
using System.Collections.Generic;
using System.Security.Cryptography;
using Org.BouncyCastle.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Electrolyte.Primitives;
using Electrolyte.Helpers;

namespace Electrolyte {
	public class Wallet {
		public static string Version = "1.0.0.0";

#if !DEBUG
		public static ulong KeyHashes = 1048576;
#else
		public static ulong KeyHashes = 1024;
#endif

		internal Timer LockTimer = new Timer();

		public bool IsLocked { get; private set; }
		public byte[] EncryptionKey = new byte[] { };
		public byte[] EncryptedData, IV, Salt;

		public Dictionary<string, byte[]> PrivateKeys;
		public HashSet<string> WatchAddresses;
		public HashSet<string> Addresses {
			get {
				HashSet<string> addresses = new HashSet<string>(PrivateKeys.Keys);
				foreach(string address in WatchAddresses) {
					if(!addresses.Contains(address))
						addresses.Add(address);
				}
				return addresses;
			}
		}

		public Wallet(byte[] passphrase) : this(passphrase, new Dictionary<string, byte[]>()) { }

		public Wallet(byte[] passphrase, Dictionary<string, byte[]> keys) : this(passphrase, keys, new HashSet<string>()) { }

		public Wallet(byte[] passphrase, Dictionary<string, byte[]> keys, HashSet<string> watchAddresses) {
			PrivateKeys = keys;
			WatchAddresses = watchAddresses;

			// TODO: Move these lines to another method?
			Lock(passphrase);
			Unlock(passphrase);
		}



		public void GenerateKey() {
			ImportKey(new ECKey());
		}

		public void ImportKey(string key, bool isPublic) {
			ImportKey(ECKey.FromWalletImportFormat(key), isPublic);
		}

		public void ImportKey(string key) {
			ImportKey(key, false);
		}

		void ImportKey(ECKey key, bool isPublic) {
			if(isPublic)
				WatchAddress(key.ToAddress());
			PrivateKeys.Add(key.ToAddress().ToString(), key.PrivateKeyBytes);
		}

		void ImportKey(ECKey key) {
			ImportKey(key, false);
		}

		public void WatchAddress(string address) {
			WatchAddresses.Add(address);
		}

		public void WatchAddress(Address address) {
			WatchAddress(address.ID);
		}



		public void Lock() {
			Lock(EncryptionKey);

			Array.Clear(EncryptionKey, 0, EncryptionKey.Length);
			EncryptionKey = new byte[] { };
		}

		public void Lock(object sender, ElapsedEventArgs e) {
			Lock();
		}

		void Lock(byte[] passphrase) {
			if(IsLocked)
				throw new InvalidOperationException("Wallet is already locked");

			Encrypt(passphrase);
			foreach(KeyValuePair<string, byte[]> privateKey in PrivateKeys) {
				Array.Clear(privateKey.Value, 0, privateKey.Value.Length);
			}

			Array.Clear(EncryptionKey, 0, EncryptionKey.Length);
			EncryptionKey = new byte[] { };

			PrivateKeys = new Dictionary<string, byte[]>();
			LockTimer.Stop();

			IsLocked = true;
		}

		void Lock(string passphrase) {
			Lock(Encoding.UTF8.GetBytes(passphrase));
		}

		public void Unlock(byte[] passphrase, double timeout) {
			if(!IsLocked)
				throw new InvalidOperationException("Wallet is already unlocked");

			Decrypt(passphrase);

			Array.Clear(EncryptedData, 0, EncryptedData.Length);
			EncryptedData = new byte[] { };

			EncryptionKey = new byte[passphrase.Length];
			Array.Copy(passphrase, EncryptionKey, passphrase.Length); // TODO: Is this safe?
			Array.Clear(passphrase, 0, passphrase.Length);

			LockTimer = new Timer(timeout);
			LockTimer.Elapsed += new ElapsedEventHandler(Lock);
			LockTimer.AutoReset = false;
			LockTimer.Start();

			IsLocked = false;
		}

		public void Unlock(byte[] passphrase) {
			Unlock(passphrase, 1000 * 60 * 15);
		}

		public void Unlock(string passphrase, double timeout) {
			Unlock(Encoding.UTF8.GetBytes(passphrase), timeout);
		}

		public void Unlock(string passphrase) {
			Unlock(Encoding.UTF8.GetBytes(passphrase));
		}



		public void LoadFromJson(string json) {
			JObject data = JObject.Parse(json);
			if(data["version"].Value<string>() != Version)
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
			IsLocked = true;
		}

		public void ReadPrivate(TextReader reader) {
			LoadPrivateDataFromJson(reader.ReadToEnd());
		}

		public void Decrypt(byte[] passphrase) {
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

		public void Decrypt(string passphrase) {
			Decrypt(Encoding.UTF8.GetBytes(passphrase));
		}

		public void Decrypt(TextReader reader, byte[] passphrase) {
			Read(reader);
			Decrypt(passphrase);
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

		public void Encrypt(byte[] passphrase) {
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

		public void Encrypt(string passphrase) {
			Encrypt(Encoding.UTF8.GetBytes(passphrase));
		}

		public void Encrypt(TextWriter writer, byte[] passphrase) {
			Encrypt(passphrase);
			Write(writer);
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
	}
}

