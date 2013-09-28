using System;
using Tiko;

namespace Electrolyte.Portable.IO {
	public abstract class PathInfo {
		public virtual string[] PathHierarchy { get; protected set; }
		public abstract string Path { get; }
		public abstract bool Exists { get; }

		protected abstract void InitializeDefaultStorage();
		protected abstract void Initialize(PathInfo location, PathInfo sublocation);
		protected abstract void Initialize(PathInfo location, params string[] sublocation);

		public abstract void Create();
		public abstract PathInfo Clone();

		public PathInfo SubPath(PathInfo location) {
			return PathInfo.Create(Clone(), location);
		}

		public PathInfo SubPath(params string[] location) {
			return PathInfo.Create(Clone(), location);
		}

		protected static PathInfo _defaultStoragePath;
		public static PathInfo DefaultStoragePath {
			get {
				if(_defaultStoragePath == null) {
					_defaultStoragePath = TikoContainer.Resolve<PathInfo>();
					_defaultStoragePath.InitializeDefaultStorage();
				}
				return _defaultStoragePath;
			}
		}

		public static PathInfo Create(PathInfo location, PathInfo sublocation) {
			var storageInfo = TikoContainer.Resolve<PathInfo>();
			storageInfo.Initialize(location, sublocation);
			return storageInfo;
		}

		public static PathInfo Create(PathInfo location, params string[] sublocation) {
			var storageInfo = TikoContainer.Resolve<PathInfo>();
			storageInfo.Initialize(location, sublocation);
			return storageInfo;
		}
	}
}

