using System;
using System.IO;
using Tiko;

namespace Electrolyte.Portable.IO {
	public abstract class FileStream : Stream {
		public virtual FileInfo File { get; protected set; }
		public virtual FileMode Mode { get; protected set; }

		protected abstract void Initialize(FileInfo info, FileMode mode);

		public static FileStream Create(FileInfo info, FileMode access) {
			var fileStream = TikoContainer.Resolve<FileStream>();
			fileStream.Initialize(info, access);
			return fileStream;
		}
	}
}

