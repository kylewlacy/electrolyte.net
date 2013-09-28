using System;

namespace Electrolyte.Portable.IO {
	public abstract class StorageInfo {
		protected abstract void InitializeDefaultStorage();
		protected abstract void Initialize(StorageInfo location, StorageInfo sublocation);
		protected abstract void Initialize(StorageInfo location, params string[] sublocation);

		public abstract StorageInfo Clone();

		public StorageInfo Append(StorageInfo location) {
			return StorageInfo.Create((StorageInfo)Clone(), location);
		}

		public StorageInfo Append(params string[] location) {
			return StorageInfo.Create((StorageInfo)Clone(), location);
		}

		protected static StorageInfo _defaultStoragePath;
		public static StorageInfo DefaultStoragePath {
			get { throw new NotImplementedException(); }
		}

		public static StorageInfo Create(StorageInfo location, StorageInfo sublocation) {
			throw new NotImplementedException();
		}

		public static StorageInfo Create(StorageInfo location, params string[] sublocation) {
			throw new NotSupportedException();
		}
	}
}

