using System;
using NUnit.Framework;
using Electrolyte;
using Electrolyte.Primitives;
using Op = Electrolyte.Script.Op;

namespace Electrolyte.Test.ScriptTest {
	[TestFixture]
	public class OpcodeTest {
		[Test]
		public void TestConstants() {
			Script @false = new Script(Op.False);
			@false.Execute();
			Assert.AreEqual(false, @false.Main.PopBool());

			Script num = new Script(Op.Num5);
			num.Execute();
			Assert.AreEqual(5, num.Main.PopInt());
		}

		[Test]
		public void TestData() {
			Script dataTiny = new Script(5, new byte[] { 0x0A, 0x0B, 0x0C, 0x0D, 0x0E });
			dataTiny.Execute();
			Assert.AreEqual(dataTiny.Main.Pop(), new byte[] { 0x0A, 0x0B, 0x0C, 0x0D, 0x0E });


			byte[] smallPack = new byte[100];
			for(int i = 0; i < smallPack.Length; i++) { smallPack[i] = (byte)i; }

			Script dataSmall = new Script(Op.PushData1, smallPack.Length, smallPack);
			dataSmall.Execute();
			Assert.AreEqual(smallPack, dataSmall.Main.Pop());


			byte[] largePack = new byte[1000];
			for(int i = 0; i < largePack.Length; i++) { largePack[i] = (byte)i; }

			Script dataLarge = new Script(Op.PushData2, largePack.Length, largePack);
			dataLarge.Execute();
			Assert.AreEqual(largePack, dataLarge.Main.Pop());
		}
	}
}

