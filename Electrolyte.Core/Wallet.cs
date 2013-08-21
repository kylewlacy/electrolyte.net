using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Timers;
using System.Collections.Generic;
using System.Security.Cryptography;
using Org.BouncyCastle.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Electrolyte.Networking;
using Electrolyte.Primitives;
using Electrolyte.Messages;
using Electrolyte.Helpers;

namespace Electrolyte {
	public class Wallet {
		public class OperationException : System.InvalidOperationException {
			public OperationException() { }
			public OperationException(string message) : base(message) { }
			public OperationException(string message, Exception inner) : base(message, inner) { }
		}

		public class LockedException : OperationException {
			public LockedException() { }
			public LockedException(string message) : base(message) { }
			public LockedException(string message, Exception inner) : base(message, inner) { }
		}

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

		public string FilePath;

		Dictionary<Address, ECKey>_privateKeys;
		public Dictionary<Address, ECKey> PrivateKeys {
			get {
				if(IsLocked) { throw new LockedException(); }
				return _privateKeys;
			}
			set {
				if(IsLocked) { throw new LockedException(); }
				_privateKeys = value;
			}
		}

		public HashSet<Address> WatchAddresses;
		public HashSet<Address> PublicAddresses;
		public HashSet<Address> Addresses {
			get {
				if(IsLocked)
					return PublicAddresses;

				HashSet<Address> addresses = new HashSet<Address>(PrivateKeys.Keys);
				foreach(Address address in PublicAddresses) {
					if(!addresses.Contains(address))
						addresses.Add(address);
				}
				return addresses;
			}
		}

		protected Wallet(string path = null) {
			PrivateKeys = new Dictionary<Address, ECKey>();
			WatchAddresses = new HashSet<Address>();
			PublicAddresses = new HashSet<Address>();
			FilePath = path;
		}

		public Wallet(byte[] passphrase) : this(passphrase, DefaultWalletPath) { }
		public Wallet(byte[] passphrase, string path) : this(passphrase, path, new Dictionary<Address, ECKey>()) { }
		public Wallet(byte[] passphrase, string path, Dictionary<Address, ECKey> keys) : this(passphrase, path, keys, new HashSet<Address>()) { }		
		public Wallet(byte[] passphrase, string path, Dictionary<Address, ECKey> keys, HashSet<Address> publicAddresses) : this(passphrase, path, keys, publicAddresses, new HashSet<Address>()) { }
		public Wallet(byte[] passphrase, string path, Dictionary<Address, ECKey> keys, HashSet<Address> publicAddresses, HashSet<Address> watchAddresses) {
			FilePath = path;
			PrivateKeys = keys;
			PublicAddresses = publicAddresses;
			WatchAddresses = watchAddresses;

			// TODO: Move these lines to another method?
			Lock(passphrase);
			Unlock(passphrase);
		}



		public Address GenerateAddress() {
			ECKey key = new ECKey();
			ImportKey(key);
			return key.ToAddress();
		}

		public void ImportKey(string key, bool isPublic = false) {
			ImportKey(ECKey.FromWalletImportFormat(key), isPublic);
		}

		public void ImportKey(ECKey key, bool isPublic = false) {
			if(IsLocked) throw new LockedException();
			if(isPublic) ImportReadOnlyAddress(key.ToAddress());
			PrivateKeys.Add(key.ToAddress(), key);
		}

		public void ImportReadOnlyAddress(string address) {
			ImportReadOnlyAddress(new Address(address));
		}

		public void ImportReadOnlyAddress(Address address) {
			if(IsLocked) throw new LockedException();
			PublicAddresses.Add(address);
			Save();
		}

		public void ImportWatchAddress(string address) {
			ImportWatchAddress(new Address(address));
		}

		public void ImportWatchAddress(Address address) {
			if(IsLocked) throw new LockedException();
			WatchAddresses.Add(address);
			Save();
		}



		public void ShowAddress(string address) {
			ShowAddress(new Address(address));
		}

		public void ShowAddress(Address address) {
			if(IsLocked) throw new LockedException();
			if(PrivateKeys.ContainsKey(address) && !PublicAddresses.Contains(address)) {
				PublicAddresses.Add(address);
				Save();
			}
			else {
				// TODO: Tailor this message for both the key not being present and the address already being public
				throw new OperationException(String.Format("Address '{0}' isnt private", address));
			}
		}

		public void HideAddress(string address) {
			HideAddress(new Address(address));
		}

		public void HideAddress(Address address) {
			if(IsLocked) throw new LockedException();
			if(PrivateKeys.ContainsKey(address) && PublicAddresses.Contains(address)) {
				PublicAddresses.Remove(address);
				Save();
			}
			else {
				// TODO: Tailor this message for both the key not being present and the address already being private
				throw new OperationException(String.Format("Address '{0}' isnt public", address));
			}
		}



		public void RemoveAddress(string address) {
			RemoveAddress(new Address(address));
		}

		public void RemoveAddress(Address address) {
			if(IsLocked) throw new LockedException();

			if(Addresses.Contains(address)) {
				if(PrivateKeys.ContainsKey(address))
					PrivateKeys.Remove(address);
				if(PublicAddresses.Contains(address))
					PublicAddresses.Remove(address);

				Save();
			}
			else {
				throw new OperationException(String.Format("Your wallet doesn't contain the address {0}", address));
			}
		}

		public void RemoveWatchAddress(string address) {
			RemoveWatchAddress(new Address(address));
		}

		public void RemoveWatchAddress(Address address) {
			if(IsLocked) throw new LockedException();

			if(WatchAddresses.Contains(address))
				WatchAddresses.Remove(address);
			else
				throw new OperationException(String.Format("You aren't watching the address '{0}'", address));

			Save();
		}



		public long GetBalance() {
			return Addresses.Select(a => Network.GetAddressBalanceAsync(a).Result).Sum();
		}

		public long GetSpendableBalance() {
			return PrivateKeys.Keys.Select(a => Network.GetAddressBalanceAsync(a).Result).Sum();
		}

		public List<Transaction.Output> GetSpendableOutputs() {
			return PrivateKeys.Keys.SelectMany(a => Network.GetUnspentOutputsAsync(a).Result).ToList();
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
				throw new OperationException("Wallet is already locked");

			Encrypt(passphrase);

			Array.Clear(EncryptionKey, 0, EncryptionKey.Length);
			EncryptionKey = new byte[] { };

			PrivateKeys = new Dictionary<Address, ECKey>(); // TODO: Is the garbage collector fast and reliable enough to make this secure?
			LockTimer.Stop();

			IsLocked = true;
		}

		void Lock(string passphrase) {
			Lock(Encoding.UTF8.GetBytes(passphrase));
		}

		public void Unlock(byte[] passphrase, double timeout) {
			if(!IsLocked)
				throw new OperationException("Wallet is already unlocked");

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



		public static Wallet Load() {
			return Wallet.Load(DefaultWalletPath);
		}

		public static Wallet Load(string path) {
			using(FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read)) {
				using(StreamReader reader = new StreamReader(stream)) {
					Wallet wallet = new Wallet(path);
					wallet.Read(reader);

					return wallet;
				}
			}
		}

		public void LoadFromJson(string json) {
			JObject data = JObject.Parse(json);
			if(data["version"].Value<string>() != Version)
				throw new FormatException(String.Format("Invalid wallet version: {0}", data["version"].Value<ulong>()));

			IV = Base58.DecodeWithChecksum(data["encrypted"]["iv"].Value<string>());
			Salt = Base58.DecodeWithChecksum(data["encrypted"]["salt"].Value<string>());

			if(data["watch_addresses"] != null) {
				foreach(JToken key in data["watch_addresses"])
					WatchAddresses.Add(new Address(key["addr"].Value<string>()));
			}

			if(data["public_addresses"] != null) {
				foreach(JToken key in data["public_addresses"])
					PublicAddresses.Add(new Address(key["addr"].Value<string>()));
			}

			EncryptedData = Base58.DecodeWithChecksum(data["encrypted"]["data"].Value<string>());
		}

		void LoadPrivateDataFromJson(string json) {
			JObject data = JObject.Parse(json);
			foreach(JToken key in data["keys"]) {
				_privateKeys.Add(new Address(key["addr"].Value<string>()), ECKey.FromWalletImportFormat(key["priv"].Value<string>()));
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
				{ "public_addresses", new List<object>() },
				{ "encrypted", new Dictionary<string, object> {
						{ "iv", Base58.EncodeWithChecksum(IV) },
						{ "salt", Base58.EncodeWithChecksum(Salt) },
						{ "data", Base58.EncodeWithChecksum(EncryptedData) }
				} }
			};

			foreach(Address address in WatchAddresses) {
				((List<object>)data["watch_addresses"]).Add(new Dictionary<string,object> {
					{ "addr", address.ToString() }
				});
			}

			foreach(Address address in PublicAddresses) {
				((List<object>)data["public_addresses"]).Add(new Dictionary<string,object> {
					{ "addr", address.ToString() }
				});
			}

			return JsonConvert.SerializeObject(data);
		}

		string PrivateDataAsJson() {
			Dictionary<string, object> data = new Dictionary<string, object>();
			data.Add("keys", new List<object>());
			foreach(KeyValuePair<Address, ECKey> privateKey in PrivateKeys) {
				((List<object>)data["keys"]).Add(new Dictionary<string, object> {
					{ "addr", privateKey.Key.ToString() },
					{ "priv", privateKey.Value.ToWalletImportFormat() }
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

		public void Save() {
			Save(FilePath);
		}

		public void Save(string path) {
			if(IsLocked) { throw new LockedException(); }

			Console.WriteLine("Trying to save...");
			if(path != null) {
				Console.WriteLine("Saving...");
				string directory = Path.GetDirectoryName(path);
				if(!Directory.Exists(directory)) { Directory.CreateDirectory(directory); }

				string backup = String.Join(".", path, "bak");
				string temp = String.Join(".", path, "new");

				if(File.Exists(temp)) { File.Delete(temp); }
				if(File.Exists(backup)) { File.Delete(backup); }
				if(File.Exists(path)) { File.Move(path, backup); }
				
				// TODO: Set proper file permissions
				using(FileStream stream = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite)) {
					using(StreamWriter writer = new StreamWriter(stream)) {
						if(!IsLocked)
							Encrypt(EncryptionKey);

						Write(writer);
					}
				}
			}
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

		public static string DefaultWalletPath {
			get {
				string storageDir;
				switch((int)Environment.OSVersion.Platform) {
				case 4:   // PlatformID.Unix
				case 128:
					storageDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".electrolyte");
					break;
				case 6:   // PlatformID.MacOSX (TODO: Create a more ambiguous version; OS X returns PlatformID.Unix)
					storageDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Application Support", "Electrolyte");
					break;
				default:
					storageDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Electrolyte");
					break;
				}

				return Path.Combine(storageDir, "wallet.json");
			}
		}
	}
}

