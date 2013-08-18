using System;
using NUnit.Framework;
using Electrolyte;

namespace Electrolyte.Test {
	[TestFixture]
	public class AddressTest {
		[Test]
		public void ParseAddress() {
			Assert.DoesNotThrow(() => {
				new Address("1JArS6jzE3AJ9sZ3aFij1BmTcpFGgN86hA");
				new Address("1VayNert3x1KzbpzMGt2qdqrAThiRovi8");
				new Address("1AGNa15ZQXAZUgFiqJ2i7Z2DPU2J6hW62i");
			});

			Assert.Throws<FormatException>(() => { new Address("N2pGWAh65TWpWmEFrFssRQkQubbczJSKi9"); });
			Assert.Throws<FormatException>(() => { new Address("1AGNa15ZQXAZUGFiqJ2i7Z2DPU2J6hW62i"); });
		}

		[Test]
		public void AddressEquality() {
			Address address1 = new Address("1JArS6jzE3AJ9sZ3aFij1BmTcpFGgN86hA");
			Address address2 = new Address("1JArS6jzE3AJ9sZ3aFij1BmTcpFGgN86hA");
			Address address3 = new Address("1VayNert3x1KzbpzMGt2qdqrAThiRovi8");
			Address address4 = new Address("1AGNa15ZQXAZUgFiqJ2i7Z2DPU2J6hW62i");

			Assert.IsTrue(address1.Equals(address2));
			Assert.IsTrue(address1 == address2);
			Assert.IsFalse(address1 != address2);

			Assert.IsFalse(address1.Equals(address3));
			Assert.IsFalse(address1 == address3);
			Assert.IsTrue(address1 != address3);

			Assert.IsFalse(address1.Equals(address4));
			Assert.IsFalse(address1 == address4);
			Assert.IsTrue(address1 != address4);

			Assert.IsFalse(address2.Equals(address3));
			Assert.IsFalse(address2 == address3);
			Assert.IsTrue(address2 != address3);

			Assert.IsFalse(address2.Equals(address4));
			Assert.IsFalse(address2 == address4);
			Assert.IsTrue(address2 != address4);

			Assert.IsFalse(address3.Equals(address4));
			Assert.IsFalse(address3 == address4);
			Assert.IsTrue(address3 != address4);
		}
	}
}

