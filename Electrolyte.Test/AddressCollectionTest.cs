using NUnit.Framework;
using System;

namespace Electrolyte.Test {
	[TestFixture]
	public class AddressCollectionTest {
		[Test]
		public void StoreAddresses() {
			var collection = new AddressCollection();

			var publicAddress = new Address("1ky1eHUrRR1kxKTbfiCptao9V25W97gDm");
			var publicDetails = new AddressDetails(publicAddress, "public");

			var miningAddress = new Address("1digVweRyrR9NbPaQJ2dfudXcZWQd81au");
			var miningDetails = new AddressDetails(miningAddress, "mining");

			var miscAddress = new Address("12pa32rAF8dejmKnU6XfXZ3aNCmroVNSQj");
			var miscDetails = new AddressDetails(miscAddress);



			collection.Add(publicDetails);
			collection.Add(miningDetails);
			collection.Add(miscAddress);

			Assert.AreEqual(collection.Count, 3);

			Assert.AreEqual(collection[0], publicDetails);
			Assert.AreEqual(collection[publicAddress], publicDetails);

			Assert.AreEqual(collection[1], miningDetails);
			Assert.AreEqual(collection[miningAddress], miningDetails);

			Assert.AreEqual(collection[2], miscDetails);
			Assert.AreEqual(collection[miscAddress], miscDetails);



			collection.Remove(publicAddress);

			Assert.AreEqual(collection.Count, 2);

			Assert.AreEqual(collection[0], miningDetails);
			Assert.AreEqual(collection[miningAddress], miningDetails);

			Assert.AreEqual(collection[1], miscDetails);
			Assert.AreEqual(collection[miscAddress], miscDetails);



			collection.Insert(1, publicDetails);

			Assert.AreEqual(collection.Count, 3);

			Assert.AreEqual(collection[0], miningDetails);
			Assert.AreEqual(collection[miningAddress], miningDetails);

			Assert.AreEqual(collection[1], publicDetails);
			Assert.AreEqual(collection[publicAddress], publicDetails);

			Assert.AreEqual(collection[2], miscDetails);
			Assert.AreEqual(collection[miscAddress], miscDetails);


			collection.RemoveAt(2);

			Assert.AreEqual(collection.Count, 2);

			Assert.AreEqual(collection[0], miningDetails);
			Assert.AreEqual(collection[miningAddress], miningDetails);

			Assert.AreEqual(collection[1], publicDetails);
			Assert.AreEqual(collection[publicAddress], publicDetails);
		}
	}
}

