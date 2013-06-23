using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;
using Org.BouncyCastle.Security;
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

		public static Wallet Decrypt(BinaryReader reader, string passpharse) {
			UInt64 version = reader.ReadUInt64();
			if(version != Version)
				throw new FormatException(String.Format("Unsupported wallet version: {0}", version));

			Aes aes = Aes.Create();
			aes.IV = reader.ReadBytes(16);
			aes.Key = PassphraseToKey(passpharse, reader.ReadString());

			Int32 length = reader.ReadInt32();

			using(MemoryStream stream = new MemoryStream(reader.ReadBytes(length))) {
				using(CryptoStream cryptoStream = new CryptoStream(stream, aes.CreateDecryptor(), CryptoStreamMode.Read))
					using(StreamReader streamReader = new StreamReader(cryptoStream))
						return Read(streamReader);
			}
		}

		public static Wallet Read(TextReader reader) {
			Dictionary<string, int> columns = new Dictionary<string, int>();
			string[] rawColumns = reader.ReadLine().Split(new char[] { ',' }, StringSplitOptions.None);
			for(int i = 0; i < rawColumns.Length; i++)
				columns.Add(rawColumns[i], i);

			List<string[]> rows = new List<string[]>();

			string rawRow;
			while(!String.IsNullOrEmpty(rawRow = reader.ReadLine())) {
				rows.Add(rawRow.Split(new char[] { ',' }, columns.Count));
			}

			Dictionary<string, byte[]> keys = new Dictionary<string, byte[]>();
			foreach(string[] row in rows) {
				ECKey key = ECKey.FromWalletImportFormat(row[columns["priv"]]);
				keys.Add(row[columns["address"]], key.PrivateKeyBytes);
			}

			return new Wallet(keys);
		}

		public Wallet() {
			PrivateKeys = new Dictionary<string, byte[]>();
		}

		public Wallet(Dictionary<string, byte[]> keys) {
			PrivateKeys = keys;
		}

		public void Encrypt(BinaryWriter writer, string passphrase, SecureRandom random) {
			writer.Write(Version);

			byte[] IV = new byte[16];
			random.NextBytes(IV);
			writer.Write(IV);

			byte[] rawSalt = new byte[128];
			random.NextBytes(rawSalt);
			string salt = Base58.EncodeWithChecksum(rawSalt);
			writer.Write(salt);

			Aes aes = Aes.Create();
			aes.Mode = CipherMode.CBC;
			aes.IV = IV;
			aes.Key = PassphraseToKey(passphrase, salt);

			using(MemoryStream stream = new MemoryStream()) {
				using(CryptoStream cryptoStream = new CryptoStream(stream, aes.CreateEncryptor(), CryptoStreamMode.Write))
					using(StreamWriter streamWriter = new StreamWriter(cryptoStream))
						Write(streamWriter);

				byte[] data = stream.ToArray();
				writer.Write(data.Length);
				writer.Write(data);
			}
		}

		public void Encrypt(BinaryWriter writer, string passphrase) {
			Encrypt(writer, passphrase, new SecureRandom());
		}

		public void Write(TextWriter writer) {
			writer.WriteLine("address,priv");
			foreach(KeyValuePair<string, byte[]> key in PrivateKeys) {
				writer.Write(String.Format("{0},{1}", key.Key, new ECKey(key.Value).ToWalletImportFormat()));
			}
			writer.WriteLine();
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

		public static byte[] PassphraseToKey(string passphrase, string salt) {
			return PassphraseToKey(Encoding.UTF8.GetBytes(passphrase), Base58.DecodeWithChecksum(salt));
		}
	}
}

