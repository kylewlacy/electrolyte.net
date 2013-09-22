using System;
using System.Threading.Tasks;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Modes;

namespace Electrolyte.Cryptography {
	public enum CipherMode {
		CBC
	};

	public enum CipherPadding {
		ISO10126d2,
		ISO7816d4,
		PKCS7,
		TBC,
		X923,
		ZeroByte
	}

	public class Cipher<T> where T : IBlockCipher, new() {
		public class DecryptionException : InvalidOperationException {
			public DecryptionException() { }
			public DecryptionException(string message) : base(message) { }
			public DecryptionException(string message, Exception inner) : base(message, inner) { }
		}

		PaddedBufferedBlockCipher cipher { get; set; }
		public CipherMode Mode { get; private set; }
		public CipherPadding Padding { get; private set; }

		public int BlockSize {
			get { return cipher.GetBlockSize(); }
		}


		public Cipher(CipherMode mode, CipherPadding padding) {
			cipher = MakeCipher(mode, padding);
			Mode = mode;
			Padding = padding;
		}

		// http://stackoverflow.com/a/5641932/1311454
		byte[] PerformCipher(bool isEncryption, byte[] data, byte[] key, byte[] iv) {
			cipher.Reset();
			cipher.Init(isEncryption, new ParametersWithIV(new KeyParameter(key), MakeIV(iv)));

			byte[] buffer = new byte[cipher.GetOutputSize(data.Length)];
			int length = cipher.ProcessBytes(data, 0, data.Length, buffer, 0);
			length += cipher.DoFinal(buffer, length);

			byte[] ciphered = new byte[length];
			Array.Copy(buffer, ciphered, length);

			return ciphered;
		}

		byte[] Encrypt(byte[] data, byte[] key, byte[] iv) {
			return PerformCipher(true, data, key, iv);
		}

		public async Task<byte[]> EncryptAsync(byte[] data, byte[] key, byte[] iv) {
			return await Task.Run(() => Encrypt(data, key, iv));
		}

		byte[] Decrypt(byte[] data, byte[] key, byte[] iv) {
			try {
				return PerformCipher(false, data, key, iv);
			}
			catch(InvalidCipherTextException) {
				throw new DecryptionException();
			}
		}

		public async Task<byte[]> DecryptAsync(byte[] data, byte[] key, byte[] iv) {
			return await Task.Run(() => Decrypt(data, key, iv));
		}

		static PaddedBufferedBlockCipher MakeCipher(CipherMode cipher, CipherPadding padding) {
			var engine = new T();
			IBlockCipher blockCipher = null;
			IBlockCipherPadding blockPadding = null;

			switch(cipher) {
			case CipherMode.CBC:
				blockCipher = new CbcBlockCipher(engine);
				break;
			}

			switch(padding) {
			case CipherPadding.ISO10126d2:
				blockPadding = new ISO10126d2Padding();
				break;
			case CipherPadding.ISO7816d4:
				blockPadding = new ISO7816d4Padding();
				break;
			case CipherPadding.PKCS7:
				blockPadding = new Pkcs7Padding();
				break;
			case CipherPadding.TBC:
				blockPadding = new TbcPadding();
				break;
			case CipherPadding.X923:
				blockPadding = new X923Padding();
				break;
			case CipherPadding.ZeroByte:
				blockPadding = new ZeroBytePadding();
				break;
			}

			return new PaddedBufferedBlockCipher(blockCipher, blockPadding);
		}

		byte[] MakeIV(byte[] iv) {
			byte[] newIV = new byte[BlockSize];
			Array.Copy(iv, newIV, Math.Min(iv.Length, newIV.Length));
			return newIV;
		}
	}

	public class AES : Cipher<AesEngine> {
		public AES(CipherMode mode, CipherPadding padding) : base(mode, padding) { }
	}
}

