using System;

namespace Electrolyte.CLI {
	static class Program {
		public static void Main(string[] args) {
			Console.WriteLine("Hello World!");

			Pause();
		}

		// TODO: Test for platform-specifics
		static void Pause() {
			Console.WriteLine("Press any key to quit...");
			Console.ReadKey();
		}
	}
}
