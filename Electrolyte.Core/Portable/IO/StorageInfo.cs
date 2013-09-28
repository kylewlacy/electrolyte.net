using System;
using Tiko;

namespace Electrolyte.Portable.IO {
	public abstract class StorageInfo {
		public virtual string[] PathHierarchy { get; protected set; }
		public abstract string Path { get; }
		public abstract bool Exists { get; }

		protected abstract void InitializeDefaultStorage();
		protected abstract void Initialize(StorageInfo location, StorageInfo sublocation);
		protected abstract void Initialize(StorageInfo location, params string[] sublocation);

		public abstract void Create();
		public abstract StorageInfo Clone();

		public StorageInfo Append(StorageInfo location) {
			return StorageInfo.Create(Clone(), location);
		}

		public StorageInfo Append(params string[] location) {
			return StorageInfo.Create(Clone(), location);
		}

		protected static StorageInfo _defaultStoragePath;
		public static StorageInfo DefaultStoragePath {
			get {
				if(_defaultStoragePath == null) {
					_defaultStoragePath = TikoContainer.Resolve<StorageInfo>();
					_defaultStoragePath.InitializeDefaultStorage();
				}
				return _defaultStoragePath;
			}
		}

		public static StorageInfo Create(StorageInfo location, StorageInfo sublocation) {
			var storageInfo = TikoContainer.Resolve<StorageInfo>();
			storageInfo.Initialize(location, sublocation);
			return storageInfo;
		}

		public static StorageInfo Create(StorageInfo location, params string[] sublocation) {
			var storageInfo = TikoContainer.Resolve<StorageInfo>();
			storageInfo.Initialize(location, sublocation);
			return storageInfo;
		}
	}
}

