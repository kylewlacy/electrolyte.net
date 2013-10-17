using NUnit.Framework;
using System;

namespace Electrolyte.Test {
	[TestFixture()]
	public class PrivateKeyCollectionTest {
		[Test()]
		public void StoreKeys() {
			var collection = new PrivateKeyCollection();

			var publicKey = new PrivateKeyDetails("5JeCapqNepVdxJEMQw6psHWd1ZZ8Qcyt8bL8XfCg2B2evNd2KHD", "public");
			var publicAddress = new Address("1JXAm1od3XxUEVeKG35gEY2pxDB9hLFY4t");

			var miningKey = new PrivateKeyDetails("5JsqyCRJsH5ygCQickog14tG7q13r6PNf35Nth2Qts2mGoU8zg8", "mining");
			var miningAddress= new Address("17XxcVFR8fc5xPXG9qW7UXYXe4uuXy93Uc");

			var miscKey = new PrivateKeyDetails("5KjxpA295xj8U6iWzSwALegxjXXB6Ln8fjDad7jfwGkosDJZycz");
			var miscAddress = new Address("1AAzCG3X3cBRBesjVVzbP8LM9eG5Rkko2S");




			collection.Add(publicKey);
			collection.Add(miningKey);
			collection.Add(miscKey);

			Assert.AreEqual(collection.Count, 3);

			Assert.AreEqual(collection[0], publicKey);
			Assert.AreEqual(collection[publicAddress], publicKey);

			Assert.AreEqual(collection[1], miningKey);
			Assert.AreEqual(collection[miningAddress], miningKey);

			Assert.AreEqual(collection[2], miscKey);
			Assert.AreEqual(collection[miscAddress], miscKey);



			collection.Remove(publicAddress);

			Assert.AreEqual(collection.Count, 2);

			Assert.AreEqual(collection[0], miningKey);
			Assert.AreEqual(collection[miningAddress], miningKey);

			Assert.AreEqual(collection[1], miscKey);
			Assert.AreEqual(collection[miscAddress], miscKey);



			collection.Insert(1, publicKey);

			Assert.AreEqual(collection.Count, 3);

			Assert.AreEqual(collection[0], miningKey);
			Assert.AreEqual(collection[miningAddress], miningKey);

			Assert.AreEqual(collection[1], publicKey);
			Assert.AreEqual(collection[publicAddress], publicKey);

			Assert.AreEqual(collection[2], miscKey);
			Assert.AreEqual(collection[miscAddress], miscKey);


			collection.RemoveAt(2);

			Assert.AreEqual(collection.Count, 2);

			Assert.AreEqual(collection[0], miningKey);
			Assert.AreEqual(collection[miningAddress], miningKey);

			Assert.AreEqual(collection[1], publicKey);
			Assert.AreEqual(collection[publicAddress], publicKey);

		}
	}
}

