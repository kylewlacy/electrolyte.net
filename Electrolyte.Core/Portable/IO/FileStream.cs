using System;
using System.IO;

namespace Electrolyte.Portable.IO {
	public abstract class FileStream : Stream {
		public virtual FileInfo File { get; private set; }

		protected abstract void Initialize(FileInfo info, FileMode access);

		public static FileStream Create() {
			throw new NotImplementedException();
		}

		public static FileStream Create(FileInfo info, FileMode access) {
			throw new NotImplementedException();
		}
	}
}

