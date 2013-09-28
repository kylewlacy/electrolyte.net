using System;
using Tiko;

namespace Electrolyte.Portable.IO {
	public abstract class FileInfo {
		public abstract StorageInfo Location { get; }
		public abstract string FileName { get; }
		public abstract bool Exists { get; }

		protected abstract void Initialize(StorageInfo location, string fileName);

		public virtual FileInfo Clone() {
			return Create(Location.Clone(), FileName);
		}

		public static FileInfo Create(StorageInfo location, string fileName) {
			var fileInfo = TikoContainer.Resolve<FileInfo>();
			fileInfo.Initialize(location, fileName);
			return fileInfo;
		}
	}
}

