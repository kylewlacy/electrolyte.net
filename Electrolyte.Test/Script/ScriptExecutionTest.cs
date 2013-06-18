using System;
using System.Security.Cryptography;
using NUnit.Framework;
using Electrolyte;
using Op = Electrolyte.Script.Op;

namespace Electrolyte.Test.ScriptTest {
	[TestFixture]
	public class OpcodeTest {
		[Test]
		public void Constants() {
			Script @false = new Script(Op.False);
			@false.Execute();
			Assert.AreEqual(false, @false.Main.PopBool());

			Script num = new Script(Op.Num5);
			num.Execute();
			Assert.AreEqual(5, num.Main.PopInt());
		}

		[Test]
		public void PushData() {
			Script dataTiny1 = new Script(5, new byte[] { 0x0A, 0x0B, 0x0C, 0x0D, 0x0E });
			dataTiny1.Execute();
			Assert.AreEqual(dataTiny1.Main.Pop(), new byte[] { 0x0A, 0x0B, 0x0C, 0x0D, 0x0E });

			Script dataTiny2 = new Script(Op.PushData1, 5, new byte[] { 0x0A, 0x0B, 0x0C, 0x0D, 0x0E });
			dataTiny2.Execute();
			Assert.AreEqual(dataTiny2.Main.Pop(), new byte[] { 0x0A, 0x0B, 0x0C, 0x0D, 0x0E });

			Script dataTiny3 = new Script(Op.PushData2, new byte[] { 5, 0 }, new byte[] { 0x0A, 0x0B, 0x0C, 0x0D, 0x0E });
			dataTiny3.Execute();
			Assert.AreEqual(dataTiny3.Main.Pop(), new byte[] { 0x0A, 0x0B, 0x0C, 0x0D, 0x0E });

			Script dataTiny4 = new Script(Op.PushData4, new byte[] { 5, 0, 0, 0 }, new byte[] { 0x0A, 0x0B, 0x0C, 0x0D, 0x0E });
			dataTiny4.Execute();
			Assert.AreEqual(dataTiny4.Main.Pop(), new byte[] { 0x0A, 0x0B, 0x0C, 0x0D, 0x0E });



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

//		[Test]
//		public void ControlFlow() {
//			Script ifTrue = new Script(Op.True, Op.If, 1, 0x0A, Op.EndIf);
//			ifTrue.Execute();
//			Assert.AreEqual(new byte[] { 0x0A }, ifTrue.Main.Pop());
//
//			Script ifFalse = new Script(Op.False, Op.If, 1, 0x0A, Op.EndIf);
//			ifFalse.Execute();
//			Assert.AreEqual(0, ifFalse.Main.Count);
//
//			Script notIfTrue = new Script(Op.True, Op.NotIf, 1, 0x0A, Op.EndIf);
//			notIfTrue.Execute();
//			Assert.AreEqual(0, notIfTrue.Main.Count);
//
//			Script notIfFalse = new Script(Op.False, Op.NotIf, 1, 0x0A, Op.EndIf);
//			notIfFalse.Execute();
//			Assert.AreEqual(new byte[] { 0x0A }, notIfFalse.Main.Pop());
//		}

		[Test]
		public void AltStack() {
			Script alt = new Script(3, new byte[] { 0x0A, 0x0B, 0x0C }, 2, new byte[] { 0x0D, 0x0E }, 1, new byte[] { 0x0F }, Op.ToAltStack, Op.ToAltStack);
			alt.Execute();
			Assert.AreEqual(new byte[] { 0x0D, 0x0E }, alt.Alt.Pop());
			Assert.AreEqual(new byte[] { 0x0F }, alt.Alt.Pop());
			Assert.AreEqual(new byte[] { 0x0A, 0x0B, 0x0C }, alt.Main.Pop());

			Script depth = new Script(1, new byte[] { 0x0A }, 2, new byte[] { 0x0B, 0x0C }, 3, new byte[] { 0x0D, 0x0E, 0x0F }, Op.Depth);
			depth.Execute();
			Assert.AreEqual(3, depth.Main.PopInt());
		}

		[Test]
		public void StackManipulation() {
			Script depth = new Script(3, new byte[] { 0x0A, 0x0B, 0x0C }, 3, new byte[] { 0x0D, 0x0E, 0x0F }, Op.Depth);
			depth.Execute();
			Assert.AreEqual(2, depth.Main.PopInt());

			Script drop = new Script(2, new byte[] { 0x0A, 0x0B }, 3, new byte[] { 0x0C, 0x0D, 0x0E }, 1, new byte[] { 0x0F }, Op.Drop);
			drop.Execute();
			Assert.AreEqual(2, drop.Main.Count);

			Script dup = new Script(2, new byte[] { 0x0A, 0x0B }, Op.Dup);
			dup.Execute();
			Assert.AreEqual(dup.Main.Pop(), dup.Main.Pop());

			Script nip = new Script(2, new byte[] { 0x0A, 0x0B }, 1, new byte[] { 0x0C }, 3, new byte[] { 0x0D, 0x0E, 0x0F }, Op.Nip);
			nip.Execute();
			Assert.AreEqual(new byte[] { 0x0D, 0x0E, 0x0F }, nip.Main.Pop());
			Assert.AreEqual(new byte[] { 0x0A, 0x0B }, nip.Main.Pop());

			Script over = new Script(2, new byte[] { 0x0A, 0x0B }, 1, new byte[] { 0x0C }, Op.Over);
			over.Execute();
			Assert.AreEqual(new byte[] { 0x0A, 0x0B }, over.Main.Pop());
			Assert.AreEqual(new byte[] { 0x0C }, over.Main.Pop());
			Assert.AreEqual(new byte[] { 0x0A, 0x0B }, over.Main.Pop());

			Script pick = new Script(2, new byte[] { 0x0A, 0x0B }, 1, new byte[] { 0x0C }, 2, new byte[] { 0x0D, 0x0E }, 1, new byte[] { 0x0F }, 1, 2, Op.Pick);
			pick.Execute();
			Assert.AreEqual(5, pick.Main.Count);
			Assert.AreEqual(new byte[] { 0x0C }, pick.Main.Pop());

			Script roll = new Script(2, new byte[] { 0x0A, 0x0B }, 1, new byte[] { 0x0C }, 2, new byte[] { 0x0D, 0x0E }, 1, new byte[] { 0x0F }, 1, 2, Op.Roll);
			roll.Execute();
			Assert.AreEqual(4, roll.Main.Count);
			Assert.AreEqual(new byte[] { 0x0C }, roll.Main.Pop());

			Script rot = new Script(2, new byte[] { 0x0A, 0x0B }, 1, new byte[] { 0x0C }, 3, new byte[] { 0x0D, 0x0E, 0x0F }, Op.Rot);
			rot.Execute();
			Assert.AreEqual(new byte[] { 0x0C }, rot.Main.Pop());
			Assert.AreEqual(new byte[] { 0x0A, 0x0B }, rot.Main.Pop());
			Assert.AreEqual(new byte[] { 0x0D, 0x0E, 0x0F }, rot.Main.Pop());

			Script swap = new Script(2, new byte[] { 0x0A, 0x0B }, 1, new byte[] { 0x0C }, 2, new byte[] { 0x0D, 0x0E }, Op.Swap);
			swap.Execute();
			Assert.AreEqual(new byte[] { 0x0C }, swap.Main.Pop());
			Assert.AreEqual(new byte[] { 0x0D, 0x0E }, swap.Main.Pop());

			Script tuck = new Script(2, new byte[] { 0x0A, 0x0B }, 1, new byte[] { 0x0C }, 2, new byte[] { 0x0D, 0x0E }, Op.Tuck);
			tuck.Execute();
			Assert.AreEqual(new byte[] { 0x0D, 0x0E }, tuck.Main.Pop());
			Assert.AreEqual(new byte[] { 0x0C }, tuck.Main.Pop());
			Assert.AreEqual(new byte[] { 0x0D, 0x0E }, tuck.Main.Pop());

			Script drop2 = new Script(1, new byte[] { 0x0A }, 2, new byte[] { 0x0B, 0x0C }, 2, new byte[] { 0x0D, 0x0E }, Op.Drop2);
			drop2.Execute();
			Assert.AreEqual(1, drop2.Main.Count);
			Assert.AreEqual(new byte[] { 0x0A }, drop2.Main.Pop());

			Script dup2 = new Script(2, new byte[] { 0x0A, 0x0B }, 1, new byte[] { 0x0C }, 2, new byte[] { 0x0D, 0x0E }, Op.Dup2);
			dup2.Execute();
			Assert.AreEqual(5, dup2.Main.Count);
			Assert.AreEqual(new byte[] { 0x0D, 0x0E }, dup2.Main.Pop());
			Assert.AreEqual(new byte[] { 0x0C }, dup2.Main.Pop());
			Assert.AreEqual(new byte[] { 0x0D, 0x0E }, dup2.Main.Pop());
			Assert.AreEqual(new byte[] { 0x0C }, dup2.Main.Pop());

			Script dup3 = new Script(2, new byte[] { 0x0A, 0x0B }, 1, new byte[] { 0x0C }, 2, new byte[] { 0x0D, 0x0E }, 1, new byte[] { 0x0F }, Op.Dup3);
			dup3.Execute();
			Assert.AreEqual(7, dup3.Main.Count);
			Assert.AreEqual(new byte[] { 0x0F }, dup3.Main.Pop());
			Assert.AreEqual(new byte[] { 0x0D, 0x0E }, dup3.Main.Pop());
			Assert.AreEqual(new byte[] { 0xC }, dup3.Main.Pop());
			Assert.AreEqual(new byte[] { 0x0F }, dup3.Main.Pop());
			Assert.AreEqual(new byte[] { 0x0D, 0x0E }, dup3.Main.Pop());
			Assert.AreEqual(new byte[] { 0xC }, dup3.Main.Pop());

			Script over2 = new Script(1, new byte[] { 0x0A }, 2, new byte[] { 0x0B, 0x0C }, 1, new byte[] { 0x0D }, 1, new byte[] { 0x0E }, 1, new byte[] { 0x0F }, Op.Over2);
			over2.Execute();
			Assert.AreEqual(7, over2.Main.Count);
			Assert.AreEqual(new byte[] { 0x0D }, over2.Main.Pop());
			Assert.AreEqual(new byte[] { 0x0B, 0x0C }, over2.Main.Pop());
			Assert.AreEqual(new byte[] { 0x0F }, over2.Main.Pop());
			Assert.AreEqual(new byte[] { 0x0E }, over2.Main.Pop());
			Assert.AreEqual(new byte[] { 0x0D }, over2.Main.Pop());
			Assert.AreEqual(new byte[] { 0x0B, 0x0C }, over2.Main.Pop());

			Script rot2 = new Script(1, new byte[] { 0x0A }, 1, new byte[] { 0x0B }, 1, new byte[] { 0x0C }, 1, new byte[] { 0x0D }, 1, new byte[] { 0x0E }, 1, new byte[] { 0x0F }, 1, new byte[] { 0x10 }, Op.Rot2);
			rot2.Execute();
			Assert.AreEqual(7, rot2.Main.Count);
			Assert.AreEqual(new byte[] { 0x0E }, rot2.Main.Pop());
			Assert.AreEqual(new byte[] { 0x0D }, rot2.Main.Pop());
			Assert.AreEqual(new byte[] { 0x0C }, rot2.Main.Pop());
			Assert.AreEqual(new byte[] { 0x0B }, rot2.Main.Pop());
			Assert.AreEqual(new byte[] { 0x10 }, rot2.Main.Pop());
			Assert.AreEqual(new byte[] { 0x0F }, rot2.Main.Pop());
			Assert.AreEqual(new byte[] { 0x0A }, rot2.Main.Pop());

			Script swap2 = new Script(2, new byte[] { 0x0A, 0x0B }, 1, new byte[] { 0x0C }, 1, new byte[] { 0x0D }, 1, new byte[] { 0x0E }, 1, new byte[] { 0x0F }, Op.Swap2);
			swap2.Execute();
			Assert.AreEqual(5, swap2.Main.Count);
			Assert.AreEqual(new byte[] { 0x0E }, swap2.Main.Pop());
			Assert.AreEqual(new byte[] { 0x0F }, swap2.Main.Pop());
			Assert.AreEqual(new byte[] { 0x0C }, swap2.Main.Pop());
			Assert.AreEqual(new byte[] { 0x0D }, swap2.Main.Pop());
			Assert.AreEqual(new byte[] { 0x0A, 0x0B }, swap2.Main.Pop());
		}

		[Test]
		public void BitwiseLogic() {
			Script equal = new Script(2, new byte[] { 0x0A, 0x0B }, 2, new byte[] { 0x0A, 0x0B }, Op.Equal);
			equal.Execute();
			Assert.IsTrue(equal.Main.IsTrue);

			Script notEqual = new Script(2, new byte[] { 0x0A, 0x0B }, 3, new byte[] { 0x0A, 0x0B, 0x0C }, Op.Equal);
			notEqual.Execute();
			Assert.IsTrue(!notEqual.Main.IsTrue);
		}

		[Test]
		public void Arithmetic() {
			Script increment = new Script(1, 8, Op.Add1);
			increment.Execute();
			Assert.AreEqual(1, increment.Main.Count);
			Assert.AreEqual(9, increment.Main.PopInt());



			Script decrement = new Script(1, 8, Op.Sub1);
			decrement.Execute();
			Assert.AreEqual(1, decrement.Main.Count);
			Assert.AreEqual(7, decrement.Main.PopInt());



			Script negatePos = new Script(1, 5, Op.Negate);
			negatePos.Execute();
			Assert.AreEqual(-5, negatePos.Main.PopInt());

			Script negateNeg = new Script(1, -5, Op.Negate);
			negateNeg.Execute();
			Assert.AreEqual(5, negateNeg.Main.PopInt());



			Script absPos = new Script(1, 5, Op.Abs);
			absPos.Execute();
			Assert.AreEqual(5, absPos.Main.PopInt());

			Script absNeg = new Script(1, -5, Op.Abs);
			absNeg.Execute();
			Assert.AreEqual(5, absNeg.Main.PopInt());
		}

		[Test]
		public void Boolean() {
			Script not0 = new Script(1, 0, Op.Not);
			not0.Execute();
			Assert.AreEqual(true, not0.Main.PopBool());

			Script not1 = new Script(1, 1, Op.Not);
			not1.Execute();
			Assert.AreEqual(false, not1.Main.PopBool());

			Script notOther = new Script(1, 50, Op.Not);
			notOther.Execute();
			Assert.AreEqual(false, notOther.Main.PopBool());



			Script notEqual0 = new Script(1, 0, Op.NotEqual0);
			notEqual0.Execute();
			Assert.AreEqual(false, notEqual0.Main.PopBool());

			Script notEqual1 = new Script(1, 1, Op.NotEqual0);
			notEqual1.Execute();
			Assert.AreEqual(true, notEqual1.Main.PopBool());

			Script notEqualOther = new Script(1, 50, Op.NotEqual0);
			notEqualOther.Execute();
			Assert.AreEqual(true, notEqualOther.Main.PopBool());



			Script add = new Script(1, 5, 1, 120, Op.Add);
			add.Execute();
			Assert.AreEqual(1, add.Main.Count);
			Assert.AreEqual(125, add.Main.PopInt());



			Script subtract = new Script(1, 25, 1, 8, Op.Sub);
			subtract.Execute();
			Assert.AreEqual(1, subtract.Main.Count);
			Assert.AreEqual(17, subtract.Main.PopInt());



			Script boolAndTrue = new Script(1, 1, 1, 1, Op.BoolAnd);
			boolAndTrue.Execute();
			Assert.AreEqual(true, boolAndTrue.Main.PopBool());

			Script boolAndFalse = new Script(1, 1, 1, 0, Op.BoolAnd);
			boolAndFalse.Execute();
			Assert.AreEqual(false, boolAndFalse.Main.PopBool());



			Script boolOrTrue = new Script(1, 1, 1, 0, Op.BoolOr);
			boolOrTrue.Execute();
			Assert.AreEqual(true, boolOrTrue.Main.PopBool());

			Script boolOrFalse = new Script(1, 0, 1, 0, Op.BoolOr);
			boolOrFalse.Execute();
			Assert.AreEqual(false, boolOrFalse.Main.PopBool());


			Script lessThanFalse = new Script(1, 2, 1, 3, Op.LessThan);
			lessThanFalse.Execute();
			Assert.AreEqual(false, lessThanFalse.Main.PopBool());

			Script lessThanTrue = new Script(1, 2, 1, -1, Op.LessThan);
			lessThanTrue.Execute();
			Assert.AreEqual(true, lessThanTrue.Main.PopBool());



			Script greaterThanFalse = new Script(1, 2, 1, -1, Op.GreaterThan);
			greaterThanFalse.Execute();
			Assert.AreEqual(false, greaterThanFalse.Main.PopBool());

			Script greaterThanTrue = new Script(1, 2, 1, 3, Op.GreaterThan);
			greaterThanTrue.Execute();
			Assert.AreEqual(true, greaterThanTrue.Main.PopBool());



			Script lessThanOrEqualFalse = new Script(1, 2, 1, 3, Op.LessThanOrEqual);
			lessThanOrEqualFalse.Execute();
			Assert.AreEqual(false, lessThanOrEqualFalse.Main.PopBool());

			Script lessThanOrEqualTrue = new Script(1, 2, 1, 2, Op.LessThanOrEqual);
			lessThanOrEqualTrue.Execute();
			Assert.AreEqual(true, lessThanOrEqualTrue.Main.PopBool());


			Script greaterThanOrEqualFalse = new Script(1, 2, 1, -1, Op.GreaterThanOrEqual);
			greaterThanOrEqualFalse.Execute();
			Assert.AreEqual(false, greaterThanOrEqualFalse.Main.PopBool());

			Script greaterThanOrEqualTrue = new Script(1, 2, 1, 2, Op.GreaterThanOrEqual);
			greaterThanOrEqualTrue.Execute();
			Assert.AreEqual(true, greaterThanOrEqualTrue.Main.PopBool());



			Script minFirst = new Script(1, 1, 1, 5, Op.Min);
			minFirst.Execute();
			Assert.AreEqual(1, minFirst.Main.PopInt());

			Script minSecond = new Script(1, 5, 1, 1, Op.Min);
			minSecond.Execute();
			Assert.AreEqual(1, minSecond.Main.PopInt());


			Script maxFirst = new Script(1, 5, 1, 1, Op.Max);
			maxFirst.Execute();
			Assert.AreEqual(5, maxFirst.Main.PopInt());

			Script maxSecond = new Script(1, 1, 1, 5, Op.Max);
			maxSecond.Execute();
			Assert.AreEqual(5, maxSecond.Main.PopInt());



			Script withinOutsideLeft = new Script(1, 5, 1, 6, 1, 8, Op.Within);
			withinOutsideLeft.Execute();
			Assert.AreEqual(false, withinOutsideLeft.Main.PopBool());

			Script withinOutsideRight = new Script(1, 9, 1, 6, 1, 8, Op.Within);
			withinOutsideRight.Execute();
			Assert.AreEqual(false, withinOutsideRight.Main.PopBool());

			Script withinInclusive = new Script(1, 6, 1, 6, 1, 8, Op.Within);
			withinInclusive.Execute();
			Assert.AreEqual(true, withinInclusive.Main.PopBool());

			Script withinExclusive = new Script(1, 8, 1, 6, 1, 8, Op.Within);
			withinExclusive.Execute();
			Assert.AreEqual(false, withinExclusive.Main.PopBool());

			Script within = new Script(1, 7, 1, 6, 1, 8, Op.Within);
			within.Execute();
			Assert.AreEqual(true, within.Main.PopBool());
		}

		[Test]
		public void Cryptography() {
			SHA1 sha1 = SHA1.Create();
			SHA256 sha256 = SHA256.Create();
			RIPEMD160 ripemd160 = RIPEMD160.Create();

			byte[] data = { 0x0A, 0x0B, 0x0C, 0x0D, 0x0E };

			Script sha1Script = new Script(data.Length, data, Op.SHA1);
			sha1Script.Execute();
			Assert.AreEqual(sha1.ComputeHash(data), sha1Script.Main.Pop());

			Script sha256Script = new Script(data.Length, data, Op.SHA256);
			sha256Script.Execute();
			Assert.AreEqual(sha256.ComputeHash(data), sha256Script.Main.Pop());

			Script ripemd160Script = new Script(data.Length, data, Op.RIPEMD160);
			ripemd160Script.Execute();
			Assert.AreEqual(ripemd160.ComputeHash(data), ripemd160Script.Main.Pop());

			Script hash160Script = new Script(data.Length, data, Op.Hash160);
			hash160Script.Execute();
			Assert.AreEqual(ripemd160.ComputeHash(sha256.ComputeHash(data)), hash160Script.Main.Pop());

			Script hash256Script = new Script(data.Length, data, Op.Hash256);
			hash256Script.Execute();
			Assert.AreEqual(sha256.ComputeHash(sha256.ComputeHash(data)), hash256Script.Main.Pop());
		}

		[Test]
		public void ExecutedScript() {
			byte[] data = { 0x0A, 0x0B, 0x0C, 0x0D, 0x0E };

			Script script = new Script(Op.Nop1, data.Length, data, Op.Nop2, data.Length, data, Op.Equal);
			script.Step();
			Assert.AreEqual(new byte[] { (byte)Op.Nop1 }, script.Executed.ToArray());
			script.Step();
			Assert.AreEqual(new byte[] { (byte)Op.Nop1, 5, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E }, script.Executed.ToArray());
			script.Step();
			Assert.AreEqual(new byte[] { (byte)Op.Nop1, 5, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, (byte)Op.Nop2 }, script.Executed.ToArray());
			script.Step();
			Assert.AreEqual(new byte[] { (byte)Op.Nop1, 5, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, (byte)Op.Nop2, 5, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E }, script.Executed.ToArray());
			script.Step();
			Assert.AreEqual(new byte[] { (byte)Op.Nop1, 5, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, (byte)Op.Nop2, 5, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, (byte)Op.Equal }, script.Executed.ToArray());
		}

		[Test]
		public void SubScript() {
			byte[] data = { 0x0A, 0x0B, 0x0C, 0x0D, 0x0E };

			Script noSeparators = new Script(data.Length, data, Op.Nop);
			noSeparators.Execute();
			Assert.AreEqual(new byte[] { 5, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, (byte)Op.Nop}, noSeparators.SubScript.Items.ToArray());

			Script separateToEnd = new Script(data.Length, data, Op.CodeSeparator, Op.Nop, data.Length, data);
			separateToEnd.Execute();
			Assert.AreEqual(new byte[] { (byte)Op.Nop, 5, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E }, separateToEnd.SubScript.Items.ToArray());

			Script multipleSeparations = new Script(data.Length, data, Op.CodeSeparator, data.Length, data, Op.CodeSeparator, Op.Nop1, data.Length, data);
			multipleSeparations.Execute();
			Assert.AreEqual(new byte[] { (byte)Op.Nop1, 5, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E }, multipleSeparations.SubScript.Items.ToArray());

			Script partialSeparation = new Script(data.Length, data, Op.CodeSeparator, Op.Nop, data.Length, data, Op.CodeSeparator, Op.Nop1, data.Length, data);
			for(int i = 0; i < 5; i++)
				partialSeparation.Step();
			Assert.AreEqual(new byte[] { (byte)Op.Nop, 5, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E }, partialSeparation.SubScript.Items.ToArray());
		}
	}
}

