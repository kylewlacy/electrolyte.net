using System;
using Tiko;
using Electrolyte.Helpers;
using Electrolyte.Portable.IO;
using AbstractFileInfo = Electrolyte.Portable.IO.FileInfo;

namespace Electrolyte.Standard.IO {
	[Resolves(typeof(AbstractFileInfo))]
	public class FileInfo : AbstractFileInfo {
		public override string Path {
			get { return System.IO.Path.Combine(Location.Path, FileName); }
		}

		public override bool Exists {
			get { return System.IO.File.Exists(Path); }
		}

		public FileInfo() { }
		protected FileInfo(Electrolyte.Portable.IO.PathInfo location, string fileName) {
			Location = location;
			FileName = fileName;
		}



		protected override void Initialize(Electrolyte.Portable.IO.PathInfo location, string fileName) {
			Location = location;
			FileName = fileName;
		}

		public override AbstractFileInfo Clone() {
			return new FileInfo(Location != null ? Location.Clone() : new PathInfo(), FileName);
		}





		public override void CopyTo(AbstractFileInfo destination) {
			if(Exists)
				System.IO.File.Copy(Path, destination.Path);
		}

		public override void MoveTo(AbstractFileInfo destination) {
			if(Exists)
				System.IO.File.Move(Path, destination.Path);
		}

		public override void Delete() {
			if(Exists)
				System.IO.File.Delete(Path);
		}
	}
}

