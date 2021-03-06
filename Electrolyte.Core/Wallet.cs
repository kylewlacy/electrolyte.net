using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Org.BouncyCastle.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Electrolyte.Networking;
using Electrolyte.Extensions;
using Electrolyte.Messages;
using Electrolyte.Helpers;
using Electrolyte.Portable;
using Electrolyte.Portable.Cryptography;
using Electrolyte.Portable.IO;
using FileInfo = Electrolyte.Portable.IO.FileInfo;
using FileStream = Electrolyte.Portable.IO.FileStream;
using FileMode = Electrolyte.Portable.IO.FileMode;
using System.Collections;

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
		static readonly SemaphoreLite privateKeyLock = new SemaphoreLite();
		static readonly SemaphoreLite saveLock = new SemaphoreLite();

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

		PrivateKeyCollection _privateKeys { get; set; }
		public PrivateKeyCollection PrivateKeys {
			get {
				if(IsLocked) { throw new LockedException(); }
				return _privateKeys;
			}
			set {
				if(IsLocked) { throw new LockedException(); }
				_privateKeys = value;
			}
		}

		public AddressCollection WatchAddresses { get; set; }

		public AddressCollection PublicAddresses { get; set; }

		public AddressCollection PrivateAddresses {
			get {
				if(IsLocked) { throw new LockedException(); }
				return new AddressCollection(Addresses.ToList().Except(PublicAddresses.ToList()).ToList());
			}
		}

		public AddressCollection Addresses {
			get {
				if(IsLocked)
					return PublicAddresses;
				return new AddressCollection(PrivateKeys.Concat(PublicAddresses).Distinct().ToList());
			}
		}

		protected Wallet(FileInfo file = null, PrivateKeyCollection keys = null, AddressCollection publicAddresses = null, AddressCollection watchAddresses = null) {
			PrivateKeys = keys ?? new PrivateKeyCollection();
			WatchAddresses = watchAddresses ?? new AddressCollection();
			PublicAddresses = publicAddresses ?? new AddressCollection();

			File = file;
		}

		public static async Task<Wallet> CreateAsync(string passphrase, FileInfo file = null, PrivateKeyCollection keys = null, AddressCollection publicAddresses = null, AddressCollection watchAddresses = null) {
			return await CreateAsync(Encoding.UTF8.GetBytes(passphrase), file, keys, publicAddresses, watchAddresses);
		}

		public static async Task<Wallet> CreateAsync(byte[] passphrase, FileInfo file = null, PrivateKeyCollection keys = null, AddressCollection publicAddresses = null, AddressCollection watchAddresses = null) {
			var wallet = new Wallet(file, keys, publicAddresses, watchAddresses);
			await wallet.LockAsync(passphrase);
			await wallet.UnlockAsync(passphrase);

			return wallet;
		}



		public async Task<PrivateKeyDetails> GenerateAddressAsync(string label = null, bool isPublic = true) {
			var key = new ECKey();
			var details = new PrivateKeyDetails(key, label);
			await ImportKeyAsync(details);
			return details;
		}

		public async Task ImportKeyAsync(string key, string label = null, bool isPublic = true) {
			await ImportKeyAsync(ECKey.FromWalletImportFormat(key), label, isPublic);
		}

		public async Task ImportKeyAsync(ECKey key, string label, bool isPublic = true) {
			await ImportKeyAsync(new PrivateKeyDetails(key, label), isPublic);
		}

		public async Task ImportKeyAsync(PrivateKeyDetails key, bool isPublic = true) {
			if(IsLocked) throw new LockedException();
			if(isPublic) await ImportReadOnlyAddressAsync(key);

			await privateKeyLock.WaitAsync();
			try {
				PrivateKeys.Add(key);
			}
			finally {
				privateKeyLock.Release();
			}
			await SaveAsync();
		}

		public async Task ImportReadOnlyAddressAsync(string address, string label = null) {
			await ImportReadOnlyAddressAsync(new Address(address), label);
		}

		public async Task ImportReadOnlyAddressAsync(Address address, string label = null) {
			await ImportReadOnlyAddressAsync(new AddressDetails(address, label));
		}

		public async Task ImportReadOnlyAddressAsync(AddressDetails addressDetails) {
			if(IsLocked) throw new LockedException();
			PublicAddresses.Add(addressDetails);
			await SaveAsync();
		}

		public async Task ImportWatchAddressAsync(string address, string label = null) {
			await ImportWatchAddressAsync(new Address(address), label);
		}

		public async Task ImportWatchAddressAsync(Address address, string label = null) {
			await ImportWatchAddressAsync(new AddressDetails(address, label));
		}

		public async Task ImportWatchAddressAsync(AddressDetails addressDetails) {
			if(IsLocked) throw new LockedException();
			WatchAddresses.Add(addressDetails);
			await SaveAsync();
		}



		public async Task ShowAddressAsync(string address) {
			await ShowAddressAsync(new Address(address));
		}

		public async Task ShowAddressAsync(Address address) {
			if(IsLocked) throw new LockedException();
			if(PrivateKeys.Contains(address) && !PublicAddresses.Contains(address)) {
				PublicAddresses.Add(PrivateKeys[address]);
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
			if(IsLocked)
				throw new LockedException();
			if(PrivateKeys.Contains(address) && PublicAddresses.Contains(address)) {
				PublicAddresses.Remove(address);
				await SaveAsync();
			}
			else {
				// TODO: Tailor this message for both the key not being present and the address already being private
				throw new OperationException(String.Format("Address '{0}' isnt public", address));
			}
		}



		public async Task SetLabelAsync(string address, string label, bool isPublic = true) {
			await SetLabelAsync(new Address(address), label, isPublic);
		}

		public async Task SetLabelAsync(Address address, string label, bool isPublic = true) {
			if(IsLocked)
				throw new LockedException();

			await privateKeyLock.WaitAsync();
			try {
				bool wasUpdated = false;
				if(PrivateAddresses.Contains(address)) {
					PrivateKeys[address].Label = label;
					wasUpdated = true;
				}

				if(isPublic && PublicAddresses.Contains(address)) {
					PublicAddresses[address].Label = label;
					wasUpdated = true;
				}

				if(!wasUpdated)
					throw new OperationException(String.Format("Could not find address '{0}' in your wallet", address));
			}
			finally {
				privateKeyLock.Release();
			}
			SaveAsync();
		}



		public async Task<Transaction> CreateTransactionAsync(Dictionary<Address, Money> destinations, Address changeAddress = null) {
			Money change;
			List<Transaction.Output> inpoints = CoinPicker.SelectInpoints(await GetSpendableOutputsAsync(), destinations, out change);
			var destinationsWithChange = new Dictionary<Address, Money>(destinations);

			if(change > new Money(0, "BTC")) {
				changeAddress = changeAddress ?? (await GenerateAddressAsync("Change")).Address;
				destinationsWithChange.Add(changeAddress, change);
			}

			Transaction tx = Transaction.Create(inpoints, destinationsWithChange, PrivateKeys.ToDictionary(k => k.Address, k => k.PrivateKey));
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
				if(PrivateKeys.Contains(address)) {
					await privateKeyLock.WaitAsync();
					try {
						PrivateKeys.Remove(address);
					}
					finally {
						privateKeyLock.Release();
					}
				}
				if(PublicAddresses.Contains(address)) {
					PublicAddresses.Remove(address);
				}

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
			return await Network.GetAddressBalancesAsync(Addresses.Addresses, startHeight);
		}

		public async Task<Money> GetCachedBalanceAsync(ulong startHeight = 0) {
			return await Network.GetCachedBalancesAsync(Addresses.Addresses, startHeight);
		}

		public async Task<Money> GetSpendableBalanceAsync(ulong startHeight = 0) {
			return await Network.GetAddressBalancesAsync(PrivateKeys.Addresses, startHeight);
		}

		public async Task<List<Transaction.Output>> GetSpendableOutputsAsync(ulong startHeight = 0) {
			return await Network.GetUnspentOutputsAsync(PrivateKeys.Addresses, startHeight);
		}


		public async Task<List<Transaction.Delta>> GetTransactionDeltasAsync(ulong startHeight = 0) {
			return await Network.GetDeltasForAddressesAsync(Addresses.Addresses, startHeight);
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

				await privateKeyLock.WaitAsync();
				try {
					PrivateKeys.Clear(); // TODO: Is this safe?
				}
				finally {
					privateKeyLock.Release();
				}

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
					LockTimer.Elapsed += LockAsync;
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
			return await Wallet.LoadAsync(DefaultWalletFile);
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
					WatchAddresses.Add(key["addr"].Value<string>(), key["label"] != null ? key["label"].Value<string>() : null);
			}

			if(data["public_addresses"] != null) {
				foreach(JToken key in data["public_addresses"])
					PublicAddresses.Add(key["addr"].Value<string>(), key["label"] != null ? key["label"].Value<string>() : null);
			}

			EncryptedData = Base58.DecodeWithChecksum(data["encrypted"]["data"].Value<string>());
		}

		async Task LoadPrivateDataFromJsonAsync(string json) {
			JObject data = JObject.Parse(json);
			foreach(JToken key in data["keys"]) {
				await privateKeyLock.WaitAsync();
				try {
					await Task.Run(() => _privateKeys.Add(key["priv"].Value<string>(), key["label"] != null ? key["label"].Value<string>() : null));
				}
				finally {
					privateKeyLock.Release();
				}
			}
		}

		public async Task ReadAsync(TextReader reader) {
			LoadFromJson(await reader.ReadToEndAsync());
			IsLocked = true;
		}

		public async Task ReadPrivateAsync(TextReader reader) {
			await LoadPrivateDataFromJsonAsync(await reader.ReadToEndAsync());
		}

		public async Task DecryptAsync(byte[] passphrase) {
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

			foreach(var address in WatchAddresses) {
				var addressJson = new JObject { { "addr", address.Address.ToString() } };
				if(!String.IsNullOrWhiteSpace(address.Label))
					addressJson.Add("label", address.Label);
				data["watch_addresses"].Value<JArray>().Add(addressJson);
			}

			foreach(var address in PublicAddresses) {
				var addressJson = new JObject { { "addr", address.Address.ToString() } };
				if(!String.IsNullOrWhiteSpace(address.Label))
					addressJson.Add("label", address.Label);
				data["public_addresses"].Value<JArray>().Add(addressJson);
			}

			return JsonConvert.SerializeObject(data);
		}

		string PrivateDataAsJson() {
			var data = new JObject { { "keys", new JArray() } };

			foreach(var privateKey in PrivateKeys) {
				var privateKeyJson = new JObject {
					{ "addr", privateKey.Address.ToString() },
					{ "priv", privateKey.PrivateKey.ToWalletImportFormat() }
				};
				if(!String.IsNullOrEmpty(privateKey.Label))
					privateKeyJson.Add("label", privateKey.Label);

				data["keys"].Value<JArray>().Add(privateKeyJson);
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
			await saveLock.WaitAsync();
			try {
				if(IsLocked) { throw new LockedException(); }

				if(file != null) {
					FileInfo backup = file.WithExtension("bak");
					FileInfo temp = file.WithExtension("new");

					temp.Delete();
					backup.Delete();
					file.MoveTo(backup);

					if(!IsLocked)
						await EncryptAsync(EncryptionKey);

					// TODO: Set proper file permissions
					using(var stream = FileStream.Create(file, FileMode.CreateNew)) {
						using(var writer = new StreamWriter(stream)) {
							await WriteAsync(writer);
						}
					}
				}
			}
			finally {
				saveLock.Release();
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



		public static FileInfo DefaultWalletFile {
			get { return FileInfo.Create(PathInfo.DefaultStoragePath, "wallet.json"); }
		}
	}
}

