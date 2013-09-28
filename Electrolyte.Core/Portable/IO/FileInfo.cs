using System;
using Tiko;

namespace Electrolyte.Portable.IO {
	public abstract class FileInfo {
		public virtual StorageInfo Location { get; protected set; }
		public virtual string FileName { get; protected set; }
		public abstract string Path { get; }
		public abstract bool Exists { get; }

		protected abstract void Initialize(StorageInfo location, string fileName);

		public abstract void CopyTo(FileInfo destination);
		public abstract void MoveTo(FileInfo destination);
		public abstract void Delete();

		public virtual FileInfo AddExtension(string extension) {
			return Create(Location.Clone(), String.Format("{0}.{1}", FileName, extension));
		}
			
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

