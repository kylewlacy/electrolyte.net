using System;

namespace Electrolyte.CLI {
	static class Program {
		public static void Main(string[] args) {
			Console.Write("Enter an address: ");
			Address address = new Address(Console.ReadLine());

			Console.WriteLine(address.ID);

			Pause();
		}

		// TODO: Test for platform-specifics
		static void Pause() {
			Console.WriteLine("Press any key to quit...");
			Console.ReadKey();
		}
	}
}
