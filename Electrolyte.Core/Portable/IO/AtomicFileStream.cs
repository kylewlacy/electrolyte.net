using System;

namespace Electrolyte.Portable.IO {
	public abstract class AtomicFileStream : FileStream {
		public virtual FileInfo BackupFile { get; private set; }
		public virtual FileInfo TempFile { get; private set; }

		protected abstract void Initialize(FileInfo info);

		public static AtomicFileStream Create(FileInfo info) {
			throw new NotImplementedException();
		}
	}
}

