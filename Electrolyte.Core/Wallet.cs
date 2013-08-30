using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Timers;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;
using Org.BouncyCastle.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Electrolyte.Networking;
using Electrolyte.Primitives;
using Electrolyte.Extensions;
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

		public class InvalidPassphraseException : OperationException {
			public InvalidPassphraseException() { }
			public InvalidPassphraseException(string message) : base(message) { }
			public InvalidPassphraseException(string message, Exception inner) : base(message, inner) { }
		}

		public static string Version = "0.0.1";

		internal Timer LockTimer = new Timer();

		public bool IsLocked { get; private set; }
		public byte[] EncryptionKey = new byte[] { };
		public byte[] EncryptedData, IV, Salt;
		public ulong KeyHashes = 1024;

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

		protected Wallet(string path = null, Dictionary<Address, ECKey> keys = null, HashSet<Address> publicAddresses = null, HashSet<Address> watchAddresses = null) {
			PrivateKeys = keys ?? new Dictionary<Address, ECKey>();
			WatchAddresses = watchAddresses ?? new HashSet<Address>();
			PublicAddresses = publicAddresses ?? new HashSet<Address>();
			FilePath = path;
		}

//		public Wallet(byte[] passphrase) : this(passphrase, DefaultWalletPath) { }
//		public Wallet(byte[] passphrase, string path) : this(passphrase, path, new Dictionary<Address, ECKey>()) { }
//		public Wallet(byte[] passphrase, string path, Dictionary<Address, ECKey> keys) : this(passphrase, path, keys, new HashSet<Address>()) { }		
//		public Wallet(byte[] passphrase, string path, Dictionary<Address, ECKey> keys, HashSet<Address> publicAddresses) : this(passphrase, path, keys, publicAddresses, new HashSet<Address>()) { }
//		public Wallet(byte[] passphrase, string path, Dictionary<Address, ECKey> keys, HashSet<Address> publicAddresses, HashSet<Address> watchAddresses) {
//			FilePath = path;
//			PrivateKeys = keys;
//			PublicAddresses = publicAddresses;
//			WatchAddresses = watchAddresses;
//
//			// TODO: Move these lines to another method?
//			Lock(passphrase);
//			await UnlockAsync(passphrase);
//		}

		public static async Task<Wallet> CreateAsync(byte[] passphrase, string path = null, Dictionary<Address, ECKey> keys = null, HashSet<Address> publicAddresses = null, HashSet<Address> watchAddresses = null) {
			Wallet wallet = new Wallet(path, keys, publicAddresses, watchAddresses);
			await wallet.LockAsync(passphrase);
			await wallet.UnlockAsync(passphrase);

			return wallet;
		}



		public async Task<Address> GenerateAddressAsync() {
			ECKey key = new ECKey();
			await ImportKeyAsync(key);
			return key.ToAddress();
		}

		public async Task ImportKeyAsync(string key, bool isPublic = true) {
			await ImportKeyAsync(ECKey.FromWalletImportFormat(key), isPublic);
		}

		public async Task ImportKeyAsync(ECKey key, bool isPublic = true) {
			if(IsLocked) throw new LockedException();
			if(isPublic) await ImportReadOnlyAddressAsync(key.ToAddress());
			PrivateKeys.Add(key.ToAddress(), key);
			await SaveAsync();
		}

		public async Task ImportReadOnlyAddressAsync(string address) {
			await ImportReadOnlyAddressAsync(new Address(address));
		}

		public async Task ImportReadOnlyAddressAsync(Address address) {
			if(IsLocked) throw new LockedException();
			PublicAddresses.Add(address);
			await SaveAsync();
		}

		public async Task ImportWatchAddressAsync(string address) {
			await ImportWatchAddressAsync(new Address(address));
		}

		public async Task ImportWatchAddressAsync(Address address) {
			if(IsLocked) throw new LockedException();
			WatchAddresses.Add(address);
			await SaveAsync();
		}



		public async Task ShowAddressAsync(string address) {
			await ShowAddressAsync(new Address(address));
		}

		public async Task ShowAddressAsync(Address address) {
			if(IsLocked) throw new LockedException();
			if(PrivateKeys.ContainsKey(address) && !PublicAddresses.Contains(address)) {
				PublicAddresses.Add(address);
				await SaveAsync();
			}
			else {
				// TODO: Tailor this message for both the key not being present and the address already being public
				throw new OperationException(String.Format("Address '{0}' isnt private", address));
			}
		}

		public async Task HideAddressAsync(string address) {
			await HideAddressAsync(new Address(address));
		}

		public async Task HideAddressAsync(Address address) {
			if(IsLocked) throw new LockedException();
			if(PrivateKeys.ContainsKey(address) && PublicAddresses.Contains(address)) {
				PublicAddresses.Remove(address);
				await SaveAsync();
			}
			else {
				// TODO: Tailor this message for both the key not being present and the address already being private
				throw new OperationException(String.Format("Address '{0}' isnt public", address));
			}
		}



		public async Task<Transaction> CreateTransactionAsync(Dictionary<Address, Money> destinations, Address changeAddress = null) {
			Money change;
			List<Transaction.Output> inpoints = CoinPicker.SelectInpoints(await GetSpendableOutputsAsync(), destinations, out change);
			Dictionary<Address, Money> destinationsWithChange = new Dictionary<Address, Money>(destinations);

			if(change > new Money(0, "BTC")) {
				changeAddress = changeAddress ?? await GenerateAddressAsync();
				Console.WriteLine("Change address: {0}", changeAddress);
				destinationsWithChange.Add(changeAddress, change);
			}
			else {
				Console.WriteLine("No change");
			}

			Console.WriteLine("Selected inpoints: ");
			foreach(Transaction.Output output in inpoints) {
				Console.WriteLine("  {0}:{1} ({2})", output.Transaction.Hash, output.Index, output.Value);
			}

			Transaction tx = Transaction.Create(inpoints, destinationsWithChange, PrivateKeys);
			if(!tx.IncludesStandardFee)
				throw new OperationException("Transaction generated without proper fee!");

			return tx;
		}



		public async Task RemoveAddressAsync(string address) {
			await RemoveAddressAsync(new Address(address));
		}

		public async Task RemoveAddressAsync(Address address) {
			if(IsLocked) throw new LockedException();

			if(Addresses.Contains(address)) {
				if(PrivateKeys.ContainsKey(address))
					PrivateKeys.Remove(address);
				if(PublicAddresses.Contains(address))
					PublicAddresses.Remove(address);

				await SaveAsync();
			}
			else {
				throw new OperationException(String.Format("Your wallet doesn't contain the address {0}", address));
			}
		}

		public async Task RemoveWatchAddressAsync(string address) {
			await RemoveWatchAddressAsync(new Address(address));
		}

		public async Task RemoveWatchAddressAsync(Address address) {
			if(IsLocked) throw new LockedException();

			if(WatchAddresses.Contains(address))
				WatchAddresses.Remove(address);
			else
				throw new OperationException(String.Format("You aren't watching the address '{0}'", address));

			await SaveAsync();
		}



		public async Task<Money> GetBalanceAsync() {
			if(Addresses.Count <= 0) return Money.Zero("BTC");
			IEnumerable<Task<Money>> balances = Addresses.Select(async (a) => await Network.GetAddressBalanceAsync(a));
			return (await Task.WhenAll(balances)).Sum();
		}

		public async Task<Money> GetSpendableBalanceAsync() {
			if(PrivateKeys.Count <= 0) return Money.Zero("BTC");
			IEnumerable<Task<Money>> balances = PrivateKeys.Keys.Select(async (a) => await Network.GetAddressBalanceAsync(a));
			return (await Task.WhenAll(balances)).Sum();
		}

		public async Task<List<Transaction.Output>> GetSpendableOutputsAsync() {
			if(PrivateKeys.Count <= 0) return new List<Transaction.Output>();

			List<Transaction.Output> outputs = new List<Transaction.Output>();
			foreach(Address address in PrivateKeys.Keys)
				outputs.AddRange(await Network.GetUnspentOutputsAsync(address));

			return outputs;
		}



		public async Task LockAsync() {
			await LockAsync(EncryptionKey);

			Array.Clear(EncryptionKey, 0, EncryptionKey.Length);
			EncryptionKey = new byte[] { };
		}

		public async void LockAsync(object sender, ElapsedEventArgs e) {
			await LockAsync();
		}

		async Task LockAsync(byte[] passphrase) {
			if(IsLocked)
				throw new OperationException("Wallet is already locked");

			await EncryptAsync(passphrase);

			Array.Clear(EncryptionKey, 0, EncryptionKey.Length);
			EncryptionKey = new byte[] { };

			PrivateKeys = new Dictionary<Address, ECKey>(); // TODO: Is the garbage collector fast and reliable enough to make this secure?
			LockTimer.Stop();

			IsLocked = true;
		}

		async void LockAsync(string passphrase) {
			await LockAsync(Encoding.UTF8.GetBytes(passphrase));
		}

		public async Task UnlockAsync(byte[] passphrase, double timeout) {
			if(!IsLocked)
				throw new OperationException("Wallet is already unlocked");

			await DecryptAsync(passphrase);

			Array.Clear(EncryptedData, 0, EncryptedData.Length);
			EncryptedData = new byte[] { };

			EncryptionKey = new byte[passphrase.Length];
			Array.Copy(passphrase, EncryptionKey, passphrase.Length); // TODO: Is this safe?
			Array.Clear(passphrase, 0, passphrase.Length);

			LockTimer = new Timer(timeout);
			LockTimer.Elapsed += new ElapsedEventHandler(LockAsync);
			LockTimer.AutoReset = false;
			LockTimer.Start();

			IsLocked = false;
		}

		public async Task UnlockAsync(byte[] passphrase) {
			await UnlockAsync(passphrase, 1000 * 60 * 15);
		}

		public async Task UnlockAsync(string passphrase, double timeout) {
			await UnlockAsync(Encoding.UTF8.GetBytes(passphrase), timeout);
		}

		public async Task UnlockAsync(string passphrase) {
			await UnlockAsync(Encoding.UTF8.GetBytes(passphrase));
		}



		public async static Task<Wallet> LoadAsync() {
			return await Wallet.LoadAsync(DefaultWalletPath);
		}

		public async static Task<Wallet> LoadAsync(string path) {
			using(FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read)) {
				using(StreamReader reader = new StreamReader(stream)) {
					Wallet wallet = new Wallet(path);
					await wallet.ReadAsync(reader);

					return wallet;
				}
			}
		}

		public void LoadFromJson(string json) {
			JObject data = JObject.Parse(json);
			if(data["version"].Value<string>() != Version)
				throw new FormatException(String.Format("Invalid wallet version: {0}", data["version"].Value<ulong>()));

			KeyHashes = data["key_hashes"].Value<ulong>();

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

		public async Task ReadAsync(TextReader reader) {
			LoadFromJson(await reader.ReadToEndAsync());
			IsLocked = true;
		}

		public async Task ReadPrivateAsync(TextReader reader) {
			LoadPrivateDataFromJson(await reader.ReadToEndAsync());
		}

		public async Task DecryptAsync(byte[] passphrase) {
			Aes aes = Aes.Create();
			aes.Mode = CipherMode.CBC;
			aes.IV = IV;
			aes.Key = await PassphraseToKeyAsync(passphrase, Salt);

			try {
				using(MemoryStream stream = new MemoryStream(EncryptedData)) {
					using(CryptoStream cryptoStream = new CryptoStream(stream, aes.CreateDecryptor(), CryptoStreamMode.Read))
						using(StreamReader streamReader = new StreamReader(cryptoStream))
							await ReadPrivateAsync(streamReader);
				}
			}
			catch(CryptographicException) {
				throw new InvalidPassphraseException();
			}
		}

		public async Task DecryptAsync(string passphrase) {
			await DecryptAsync(Encoding.UTF8.GetBytes(passphrase));
		}

		public async Task DecryptAsync(TextReader reader, byte[] passphrase) {
			await ReadAsync(reader);
			await DecryptAsync(passphrase);
		}

		public async Task DecryptAsync(TextReader reader, string passphrase) {
			await ReadAsync(reader);
			await DecryptAsync(passphrase);
		}

		public string DataAsJson() {
			Dictionary<string, object> data = new Dictionary<string, object> {
				{ "version", Version },
				{ "watch_addresses", new List<object>() },
				{ "public_addresses", new List<object>() },
				{ "key_hashes", KeyHashes },
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

		public async Task WriteAsync(TextWriter writer) {
			await writer.WriteAsync(DataAsJson());
		}

		public async Task WritePrivateAsync(TextWriter writer) {
			await writer.WriteAsync(PrivateDataAsJson());
		}

		public async Task EncryptAsync(byte[] passphrase) {
			SecureRandom random = new SecureRandom();

			IV = new byte[16];
			random.NextBytes(IV);

			Salt = new byte[128];
			random.NextBytes(Salt);

			Aes aes = Aes.Create();
			aes.Mode = CipherMode.CBC;
			aes.IV = IV;
			aes.Key = await PassphraseToKeyAsync(passphrase, Salt);

			using(MemoryStream stream = new MemoryStream()) {
				using(CryptoStream cryptoStream = new CryptoStream(stream, aes.CreateEncryptor(), CryptoStreamMode.Write))
					using(StreamWriter streamWriter = new StreamWriter(cryptoStream))
						await WritePrivateAsync(streamWriter);
				EncryptedData = stream.ToArray();
			}
		}

		public async Task EncryptAsync(string passphrase) {
			await EncryptAsync(Encoding.UTF8.GetBytes(passphrase));
		}

		public async Task EncryptAsync(TextWriter writer, byte[] passphrase) {
			await EncryptAsync(passphrase);
			await WriteAsync(writer);
		}

		public async Task EncryptAsync(TextWriter writer, string passphrase) {
			await EncryptAsync(passphrase);
			await WriteAsync(writer);
		}

		public async Task SaveAsync() {
			await SaveAsync(FilePath);
		}

		public async Task SaveAsync(string path) {
			if(IsLocked) { throw new LockedException(); }

			if(path != null) {
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
							await EncryptAsync(EncryptionKey);

						await WriteAsync(writer);
					}
				}
			}
		}



		static async Task<byte[]> PassphraseToKeyAsync(byte[] passphrase, byte[] salt, ulong keyHashes) {
			byte[] key = passphrase;

			await Task.Run(() => {
				using(SHA256 sha256 = SHA256.Create()) {
					for(ulong i = 0; i < keyHashes; i++) {
						key = sha256.ComputeHash(ArrayHelpers.ConcatArrays(key, passphrase, salt));
					}
				}
			});

			return ArrayHelpers.SubArray(key, 0, 32);
		}

		async Task<byte[]> PassphraseToKeyAsync(byte[] passphrase, byte[] salt) {
			return await PassphraseToKeyAsync(passphrase, salt, KeyHashes);
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

