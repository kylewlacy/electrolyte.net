using System;
using Electrolyte;
using NUnit.Framework;

namespace Electrolyte.Test.ScriptTest {
	[TestFixture]
	public class ScriptInterpretationTest {
		Tuple<string, byte[], object[]>[] scripts = new Tuple<string, byte[], object[]>[] {
			Tuple.Create(
				"OP_TRUE OP_IF 0A0B0C0D0E OP_ELSE 0F10111213 OP_ENDIF",
			    new byte[] { (byte)Op.True, (byte)Op.If, 5, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, (byte)Op.Else, 5, 0x0F, 0x10, 0x11, 0x12, 0x13, (byte)Op.EndIf },
				new object[] { Op.True, Op.If, new byte[] { 0x0A, 0x0B, 0x0C, 0x0D, 0x0E }, Op.Else, new byte[] { 0x0F, 0x10, 0x11, 0x12, 0x13 }, Op.EndIf }
			)
		};

		[Test]
		public void FromString() {
			foreach(Tuple<string, byte[], object[]> script in scripts) {
				Assert.AreEqual(new Script(script.Item2).Execution.Items, Script.FromString(script.Item1).Execution.Items);
			}
		}

		[Test]
		new public void ToString() {
			foreach(Tuple<string, byte[], object[]> script in scripts) {
				Assert.AreEqual(script.Item1, new Script(script.Item2).ToString());
			}
		}

		[Test]
		public void FromObjectArray() {
			foreach(Tuple<string, byte[], object[]> script in scripts) {
				Assert.AreEqual(new Script(script.Item2).Execution.Items, Script.Create(script.Item3).Execution.Items);
			}
		}
	}
}

