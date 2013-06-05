using System;
using NUnit.Framework;
using Electrolyte;

namespace Electrolyte.Test {
	[TestFixture]
	public class AddressTest {
		[Test]
		public void AddressFormatTest() {
			Assert.DoesNotThrow(() => {
				new Address("1dice8EMZmqKvrGE4Qc9bUFf9PX3xaYDp");
				new Address("31uEbMgunupShBVTewXjtqbBv5MndwfXhb");
				new Address("1AGNa15ZQXAZUgFiqJ2i7Z2DPU2J6hW62i");
				new Address("1dice8EMZmqKvrGE4Qc9bUFf9PX3xaYDp");
				new Address("1dice8EMZmqKvrGE4Qc9bUFf9PX3xaYDp");
				new Address("1dice8EMZmqKvrGE4Qc9bUFf9PX3xaYDpa");
			});

			Assert.Throws<InvalidAddressException>(() => { new Address("2AGNa15ZQXAZUgFiqJ2i7Z2DPU2J6hW62i"); });
			Assert.Throws<InvalidAddressException>(() => { new Address("AGNa15ZQXAZUgFiqJ2i7Z2DPU2J6hW62i"); });
			Assert.Throws<InvalidAddressException>(() => { new Address("1AGNa15ZQXAZUgFiqJ2i7Z2DPU2J6hW62I"); });
			Assert.Throws<InvalidAddressException>(() => { new Address("2AGNa15ZQXAZUgFiqJ2i7Z2DPU2J6hW62O"); });
			Assert.Throws<InvalidAddressException>(() => { new Address("2AGNa15ZQXAZUgFiqJ2i7Z2DPU2J6hW62l"); });
			Assert.Throws<InvalidAddressException>(() => { new Address("1AGN"); });
			Assert.Throws<InvalidAddressException>(() => { new Address("1AGNa15ZQXAZUgFiqJ2i7Z2DPU2J6hW62iiiiiiiiiiii"); });
			Assert.Throws<InvalidAddressException>(() => { new Address("1dice8EMZmqKvrGE4Qc9bUFf9P"); });
			Assert.Throws<InvalidAddressException>(() => { new Address("1dice8EMZmqKvrGE4Qc9bUFf9PX3xaYDpai"); });
		}
	}
}

