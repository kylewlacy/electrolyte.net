using System;
using NUnit.Framework;
using Electrolyte;

namespace Electrolyte.Test {
	[TestFixture]
	public class AddressTest {
		[Test]
		public void AddressFormatTest() {
			Assert.DoesNotThrow(() => {
				new Address("1JArS6jzE3AJ9sZ3aFij1BmTcpFGgN86hA");
				new Address("1VayNert3x1KzbpzMGt2qdqrAThiRovi8");
				new Address("1AGNa15ZQXAZUgFiqJ2i7Z2DPU2J6hW62i");
			});

			Assert.Throws<FormatException>(() => { new Address("N2pGWAh65TWpWmEFrFssRQkQubbczJSKi9"); });
			Assert.Throws<FormatException>(() => { new Address("1AGNa15ZQXAZUGFiqJ2i7Z2DPU2J6hW62i"); });
		}
	}
}

