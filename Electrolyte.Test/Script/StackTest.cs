using System;
using System.Collections.Generic;
using NUnit.Framework;
using Stack = Electrolyte.Script.Stack<byte>;

namespace Electrolyte.Test.Script {
	[TestFixture]
	public class StackTest {
		[Test]
		public void LIFO() {
			Stack stack = new Stack();
			stack.Push(0x01);
			stack.Push(0x02);
			stack.Push(0x03);

			Assert.AreEqual(0x03, stack.Pop());
			Assert.AreEqual(0x02, stack.Pop());
			Assert.AreEqual(0x01, stack.Pop());

			stack = new Stack();
			stack.Push(new byte[] { 0x01, 0x02, 0x03 });
			Assert.AreEqual(new byte[] { 0x03, 0x02, 0x01 }, stack.Pop(3));
		}

		[Test]
		public void Ordering() {
			Stack stack = new Stack(new byte[] { 0x01, 0x02, 0x03 });
			stack.Push(new byte[] {0x04, 0x05, 0x06});
			Assert.AreEqual(0x06, stack.Pop());
			stack.Push(0x07);
			Assert.AreEqual(new byte[] { 0x07, 0x05 }, stack.Pop(2));
			Assert.AreEqual(new byte[] { 0x04, 0x03, 0x02 }, stack.Pop(3));
			stack.Push(new byte[] { 0x08, 0x09, 0x0A });
			Assert.AreEqual(new byte[] { 0x01, 0x08, 0x09, 0x0A }, stack.ToArray());
		}

		[Test]
		public void Index() {
			Stack stack = new Stack();
			stack.Push(new byte[] { 0x01, 0x02, 0x03 });

			Assert.AreEqual(0x03, stack[0]);
			Assert.AreEqual(0x02, stack[1]);
			Assert.AreEqual(0x01, stack[2]);
		}

		[Test]
		public void Conversion() {
			Stack stack = new Stack();
			stack.Push(0x01);
			stack.Push(0x02);
			stack.Push(0x03);

			Assert.AreEqual(new byte[] { 0x01, 0x02, 0x03, }, stack.ToArray());
			Assert.AreEqual(new List<byte> { 0x01, 0x02, 0x03 }, stack.ToList());

			Assert.AreEqual(stack.ToArray(), new Stack(stack.ToArray()).ToArray());
		}
	}
}

