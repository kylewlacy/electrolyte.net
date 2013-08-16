using System;
using Electrolyte;

namespace Electrolyte.CLI {
	class MainClass {
		public static void Main(string[] args) {
			Console.Write("Enter an address: ");
			Address address = new Address(Console.ReadLine());

			Console.WriteLine("Address is {0}", address.ID);


		}
	}
}
