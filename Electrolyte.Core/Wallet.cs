using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Org.BouncyCastle.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Electrolyte.Portable.Cryptography;
using Electrolyte.Portable.IO;
using Electrolyte.Networking;
using Electrolyte.Portable;
using Electrolyte.Messages;
using Electrolyte.Helpers;
using FileInfo = Electrolyte.Portable.IO.FileInfo;
using FileStream = Electrolyte.Portable.IO.FileStream;
using FileMode = Electrolyte.Portable.IO.FileMode;

namespace Electrolyte {
	public class Wallet {
		public class OperationException : InvalidOperationException {
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

		internal Timer LockTimer = Timer.Create();

		// http://stackoverflow.com/a/340618
		public event EventHandler DidLock = delegate { };
		public event EventHandler DidUnlock = delegate { };
		public event EventHandler DidFailToUnlock = delegate { };
		
		static readonly SemaphoreLite lockLock = new SemaphoreLite();
		public bool IsLocked { get; private set; }

		public byte[] EncryptionKey = { };
		public byte[] EncryptedData, IV, Salt;
		public ulong KeyHashes = 1024;

		public FileInfo File;

		AES _aes;
		public AES AES {
			get { return _aes ?? (_aes = new AES(CipherMode.CBC, CipherPadding.PKCS7)); }
			set { _aes = value; }
		}

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

		public HashSet<Address> PrivateAddresses {
			get {
				if(IsLocked) { throw new LockedException(); }
				return new HashSet<Address>(Addresses.Except(PublicAddresses));
			}
		}

		public HashSet<Address> Addresses {
			get {
				if(IsLocked)
					return PublicAddresses;

				var addresses = new HashSet<Address>(PrivateKeys.Keys);
				foreach(Address address in PublicAddresses) {
					if(!addresses.Contains(address))
						addresses.Add(address);
				}
				return addresses;
			}
		}

		protected Wallet(FileInfo file = null, Dictionary<Address, ECKey> keys = null, HashSet<Address> publicAddresses = null, HashSet<Address> watchAddresses = null) {
			PrivateKeys = keys ?? new Dictionary<Address, ECKey>();
			WatchAddresses = watchAddresses ?? new HashSet<Address>();
			PublicAddresses = publicAddresses ?? new HashSet<Address>();
			File = file;
		}

		public static async Task<Wallet> CreateAsync(byte[] passphrase, FileInfo file = null, Dictionary<Address, ECKey> keys = null, HashSet<Address> publicAddresses = null, HashSet<Address> watchAddresses = null) {
			var wallet = new Wallet(file, keys, publicAddresses, watchAddresses);
			await wallet.LockAsync(passphrase);
			await wallet.UnlockAsync(passphrase);

			return wallet;
		}



		public async Task<Address> GenerateAddressAsync() {
			var key = new ECKey();
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
			var destinationsWithChange = new Dictionary<Address, Money>(destinations);

			if(change > new Money(0, "BTC")) {
				changeAddress = changeAddress ?? await GenerateAddressAsync();
				destinationsWithChange.Add(changeAddress, change);
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



		public async Task<Money> GetBalanceAsync(ulong startHeight = 0) {
			return await Network.GetAddressBalancesAsync(Addresses.ToList(), startHeight);
		}

		public async Task<Money> GetCachedBalanceAsync(ulong startHeight = 0) {
			return await Network.GetCachedBalancesAsync(Addresses.ToList(), startHeight);
		}

		public async Task<Money> GetSpendableBalanceAsync(ulong startHeight = 0) {
			return await Network.GetAddressBalancesAsync(PrivateKeys.Keys.ToList(), startHeight);
		}

		public async Task<List<Transaction.Output>> GetSpendableOutputsAsync(ulong startHeight = 0) {
			return await Network.GetUnspentOutputsAsync(PrivateKeys.Keys.ToList(), startHeight);
		}


		public async Task<List<Transaction.Delta>> GetTransactionDeltasAsync(ulong startHeight = 0) {
			return await Network.GetDeltasForAddressesAsync(Addresses.ToList(), startHeight);
		}



		public async Task LockAsync() {
			await LockAsync(EncryptionKey);

			Array.Clear(EncryptionKey, 0, EncryptionKey.Length);
			EncryptionKey = new byte[] { };
		}

		public async void LockAsync(object sender, EventArgs e) {
			await LockAsync();
		}

		async Task LockAsync(byte[] passphrase) {
			await lockLock.WaitAsync();
			try {
				if(IsLocked)
					throw new OperationException("Wallet is already locked");

				await EncryptAsync(passphrase);

				Array.Clear(EncryptionKey, 0, EncryptionKey.Length);
				EncryptionKey = new byte[] { };

				PrivateKeys = new Dictionary<Address, ECKey>(); // TODO: Is the garbage collector fast and reliable enough to make this secure?
				LockTimer.Stop();

				IsLocked = true;
				DidLock(this, new EventArgs());
			}
			finally {
				lockLock.Release();
			}
		}

		async void LockAsync(string passphrase) {
			await LockAsync(Encoding.UTF8.GetBytes(passphrase));
		}

		public async Task UnlockAsync(byte[] passphrase, TimeSpan timeout) {
			await lockLock.WaitAsync();
			try {
				if(!IsLocked)
					throw new OperationException("Wallet is already unlocked");

				try {
					await DecryptAsync(passphrase);

					Array.Clear(EncryptedData, 0, EncryptedData.Length);
					EncryptedData = new byte[] { };

					EncryptionKey = new byte[passphrase.Length];
					Array.Copy(passphrase, EncryptionKey, passphrase.Length); // TODO: Is this safe?
					Array.Clear(passphrase, 0, passphrase.Length);

					LockTimer = Timer.Create(timeout);
					LockTimer.Elapsed += new EventHandler(LockAsync);
					LockTimer.Start();

					IsLocked = false;
					DidUnlock(this, new EventArgs());
				}
				catch {
					DidFailToUnlock(this, new EventArgs());
					throw;
				}
			}
			finally {
				lockLock.Release();
			}
		}

		public async Task UnlockAsync(byte[] passphrase) {
			await UnlockAsync(passphrase, TimeSpan.FromMinutes(15));
		}

		public async Task UnlockAsync(string passphrase, TimeSpan timeout) {
			await UnlockAsync(Encoding.UTF8.GetBytes(passphrase), timeout);
		}

		public async Task UnlockAsync(string passphrase) {
			await UnlockAsync(Encoding.UTF8.GetBytes(passphrase));
		}



		public async static Task<Wallet> LoadAsync() {
			return await Wallet.LoadAsync(DefaultWalletPath);
		}

		public async static Task<Wallet> LoadAsync(FileInfo fileInfo) {
			using(var stream = FileStream.Create(fileInfo, FileMode.Open)) {
				using(var reader = new StreamReader(stream)) {
					var wallet = new Wallet(fileInfo);
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
//			Aes aes = Aes.Create();
//			aes.Mode = CipherMode.CBC;
//			aes.IV = IV;
//			aes.Key = await PassphraseToKeyAsync(passphrase, Salt);
//
//			try {
//				using(var stream = new MemoryStream(EncryptedData)) {
//					using(var cryptoStream = new CryptoStream(stream, aes.CreateDecryptor(), CryptoStreamMode.Read))
//						using(var streamReader = new StreamReader(cryptoStream))
//							await ReadPrivateAsync(streamReader);
//				}
//			}
//			catch(CryptographicException) {
//				throw new InvalidPassphraseException();
//			}

			byte[] decryptedData;
			try {
				decryptedData = await AES.DecryptAsync(EncryptedData, await PassphraseToKeyAsync(passphrase, Salt), IV);
			}
			catch(AES.DecryptionException) {
				throw new InvalidPassphraseException();
			}

			using(var stream = new MemoryStream(decryptedData)) {
				using(var streamReader = new StreamReader(stream)) {
					await ReadPrivateAsync(streamReader);
				}
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
			var data = new JObject {
				{ "version", Version },
				{ "watch_addresses", new JArray() },
				{ "public_addresses", new JArray() },
				{ "key_hashes", KeyHashes },
				{ "encrypted", new JObject {
						{ "iv", Base58.EncodeWithChecksum(IV) },
						{ "salt", Base58.EncodeWithChecksum(Salt) },
						{ "data", Base58.EncodeWithChecksum(EncryptedData) }
				} }
			};

			foreach(Address address in WatchAddresses) {
				data["watch_addresses"].Value<JArray>().Add(new JObject {
					{ "addr", address.ToString() }
				});
			}

			foreach(Address address in PublicAddresses) {
				data["public_addresses"].Value<JArray>().Add(new JObject {
					{ "addr", address.ToString() }
				});
			}

			return JsonConvert.SerializeObject(data);
		}

		string PrivateDataAsJson() {
			var data = new JObject { { "keys", new JArray() } };

			foreach(KeyValuePair<Address, ECKey> privateKey in PrivateKeys) {
				data["keys"].Value<JArray>().Add(new JObject {
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
			var random = new SecureRandom();

			IV = new byte[256];
			random.NextBytes(IV);

			Salt = new byte[128];
			random.NextBytes(Salt);

			using(var stream = new MemoryStream()) {
				using(var streamWriter = new StreamWriter(stream))
					await WritePrivateAsync(streamWriter);
				EncryptedData = await AES.EncryptAsync(stream.ToArray(), await PassphraseToKeyAsync(passphrase, Salt), IV);
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
			await SaveAsync(File);
		}

		public async Task SaveAsync(FileInfo file) {
			if(IsLocked) { throw new LockedException(); }

			if(file != null) {
				if(!IsLocked)
					await EncryptAsync(EncryptionKey);

				// TODO: Set proper file permissions
				using(var stream = AtomicFileStream.Create(file)) {
					using(var writer = new StreamWriter(stream)) {
						await WriteAsync(writer);
					}
				}
			}
		}



		static async Task<byte[]> PassphraseToKeyAsync(byte[] passphrase, byte[] salt, ulong keyHashes) {
			byte[] key = passphrase;

			await Task.Run(() => {
				for(ulong i = 0; i < keyHashes; i++) {
					key = SHA256.Hash(ArrayHelpers.ConcatArrays(key, passphrase, salt));
				}
			});

			return ArrayHelpers.SubArray(key, 0, 32);
		}

		async Task<byte[]> PassphraseToKeyAsync(byte[] passphrase, byte[] salt) {
			return await PassphraseToKeyAsync(passphrase, salt, KeyHashes);
		}



		public static FileInfo DefaultWalletPath {
			get { return FileInfo.Create(StorageInfo.DefaultStoragePath, "wallet.json"); }
		}
	}
}

