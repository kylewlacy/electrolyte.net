using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;

namespace Electrolyte.Primitives.Cryptography {
	public abstract class Digest {
		protected Digest() { }

		protected abstract byte[] HashData(byte[] data);

		public static byte[] Hash<T>(byte[] data) where T : Digest, new() {
			return (new T()).HashData(data);
		}

		public static byte[] Hash<T, U>(byte[] data) where T : Digest, new() where U : Digest, new() {
			return (new T()).HashData((new U()).HashData(data));
		}
	}

	public class Digest<T> : Digest where T : IDigest, new() {
		static readonly T digest;

		static Digest() {
			digest = new T();
		}

		public static byte[] Hash(byte[] data) {
			var buffer = new byte[digest.GetDigestSize()];
			digest.BlockUpdate(data, 0, data.Length);
			digest.DoFinal(buffer, 0);

			return buffer;
		}

		protected override byte[] HashData(byte[] data) {
			return Hash(data);
		}
	}
	
	public class MD2 : Digest<MD2Digest> { }
	public class MD4 : Digest<MD4Digest> { }
	public class MD5 : Digest<MD5Digest> { }
	public class SHA1 : Digest<Sha1Digest> { }
	public class SHA224 : Digest<Sha224Digest> { }
	public class SHA256 : Digest<Sha256Digest> { }
	public class SHA384 : Digest<Sha384Digest> { }
	public class SHA512 : Digest<Sha512Digest> { }
	public class RIPEMD128 : Digest<RipeMD128Digest> { }
	public class RIPEMD160 : Digest<RipeMD160Digest> { }
	public class RIPEMD256 : Digest<RipeMD256Digest> { }
	public class RIPEMD320 : Digest<RipeMD320Digest> { }
}

