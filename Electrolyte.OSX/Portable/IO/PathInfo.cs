using System;
using System.Linq;
using System.Collections.Generic;
using MonoMac.Foundation;
using MonoMac.AppKit;
using Tiko;
using Electrolyte.Portable.IO;
using BasePathInfo = Electrolyte.Standard.IO.PathInfo;
using AbstractPathInfo = Electrolyte.Portable.IO.PathInfo;

namespace Electrolyte.OSX.Portable.IO {
	[Resolves(typeof(AbstractPathInfo))]
	public class PathInfo : BasePathInfo {
		const char separationCharacter = '/';

		protected override void InitializeDefaultStorage() {
			List<string> pathHierarchy = new List<string>();
			pathHierarchy.AddRange(NSSearchPath.GetDirectories(NSSearchPathDirectory.ApplicationSupportDirectory, NSSearchPathDomain.User, true)[0].Split(separationCharacter));
			pathHierarchy.Add("Electrolyte");

			PathHierarchy = pathHierarchy.ToArray();
		}
	}
}

