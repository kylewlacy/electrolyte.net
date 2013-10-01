using System;
using Tiko;

namespace Electrolyte.Portable.IO {
	public abstract class FileInfo {
		public virtual PathInfo Location { get; protected set; }
		public virtual string FileName { get; protected set; }
		public abstract string Path { get; }
		public abstract bool Exists { get; }

		protected abstract void Initialize(PathInfo location, string fileName);

		public abstract void CopyTo(FileInfo destination);
		public abstract void MoveTo(FileInfo destination);
		public abstract void Delete();

		public virtual FileInfo WithExtension(string extension) {
			var newFileInfo = Clone();
			newFileInfo.AddExtension(extension);
			return newFileInfo;
		}

		public virtual void AddExtension(string extension) {
			FileName = String.Format("{0}.{1}", FileName, extension);
		}
			
		public abstract FileInfo Clone();

		public static FileInfo Create(PathInfo location, string fileName) {
			var fileInfo = TikoContainer.Resolve<FileInfo>().Clone();
			fileInfo.Initialize(location, fileName);
			return fileInfo;
		}
	}
}

