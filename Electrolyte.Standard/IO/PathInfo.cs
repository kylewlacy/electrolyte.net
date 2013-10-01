using System;
using System.Collections.Generic;
using Tiko;
using Electrolyte.Helpers;
using AbstractPathInfo = Electrolyte.Portable.IO.PathInfo;

namespace Electrolyte.Standard.IO {
	[Resolves(typeof(AbstractPathInfo))]
	public class PathInfo : AbstractPathInfo {
		public override string Path {
			get { return System.IO.Path.Combine(PathHierarchy); }
		}

		public override bool Exists {
			get { return System.IO.Directory.Exists(Path); }
		}

		public PathInfo() { }
		protected PathInfo(params string[] pathHierarchy) {
			PathHierarchy = pathHierarchy;
		}

		protected override void Initialize(AbstractPathInfo location) {
			PathHierarchy = location.PathHierarchy;
		}

		protected override void Initialize(AbstractPathInfo location, params string[] sublocation) {
			List<string> pathHierarchy = new List<string>();
			pathHierarchy.AddRange(location.PathHierarchy);
			pathHierarchy.AddRange(sublocation);
			PathHierarchy = pathHierarchy.ToArray();
		}

		protected override void Initialize(AbstractPathInfo location, AbstractPathInfo sublocation) {
			Initialize(location, sublocation.PathHierarchy);
		}

		protected override void InitializeDefaultStorage() {
			List<string> pathHierarchy = new List<string>();
			pathHierarchy.AddRange(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData).Split(System.IO.Path.PathSeparator));
			pathHierarchy.Add("Electrolyte");

			PathHierarchy = pathHierarchy.ToArray();
		}

		public override void Create() {
			if(!Exists)
				System.IO.Directory.CreateDirectory(Path);
		}

		public override AbstractPathInfo Clone() {
			string[] newPathHierarchy = new string[PathHierarchy.LongLength];
			Array.Copy(PathHierarchy, newPathHierarchy, PathHierarchy.LongLength);
			return new PathInfo(newPathHierarchy);
		}
	}
}

