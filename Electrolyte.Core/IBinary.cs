using System;
using System.IO;

namespace Electrolyte {
	public interface IBinary {
		void Write(BinaryWriter writer);
	}
}

