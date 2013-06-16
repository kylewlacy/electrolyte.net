using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using Electrolyte.Primitives;

namespace Electrolyte {
	public partial class Script {
		public DataStack Main, Alt;
		public Stack<byte> Execution;

		public Script(byte[] script) {
			Execution = new Stack<byte>(script);
			Main = new DataStack();
			Alt = new DataStack();
		}

		public Script(params object[] script) {
			List<byte> bytes = new List<byte>();
			foreach(object o in script.Reverse()) {
				if(o is byte[])
					bytes.AddRange(((byte[])o).Reverse());
				else if(o is int)
					bytes.AddRange(new SignedInt((int)o).ToByteArray().Reverse().ToArray());
				else if(o is SignedInt)
					bytes.AddRange(((SignedInt)o).ToByteArray().Reverse().ToArray());
				else
					bytes.Add((byte)o);
			}

			Execution = new Stack<byte>(bytes.ToArray());
			Main = new DataStack();
			Alt = new DataStack();
		}

		public void Step() {
			byte next = Execution.Pop();
			if(1 <= next && next <= 75) {
				Main.Push(Execution.Pop(next));
			}
			else {
				switch((Op)next) {
				case Op.False:
					Main.Push(false);
					break;
				case Op.PushData1:
					int i = new SignedInt(Execution.Pop()).Value;
					Console.WriteLine(i);
					Main.Push(Execution.Pop(i));
					break;
				case Op.PushData2:
					Main.Push(Execution.Pop(new SignedInt(Execution.Pop(2)).Value));
					break;
				case Op.PushData4:
					Main.Push(Execution.Pop(new SignedInt(Execution.Pop(4)).Value));
					break;
				case Op.Negate1:
					Main.Push(-1);
					break;
				case Op.True:
					Main.Push(true);
					break;
				case Op.Num2:
					Main.Push(2);
					break;
				case Op.Num3:
					Main.Push(3);
					break;
				case Op.Num4:
					Main.Push(4);
					break;
				case Op.Num5:
					Main.Push(5);
					break;
				case Op.Num6:
					Main.Push(6);
					break;
				case Op.Num7:
					Main.Push(7);
					break;
				case Op.Num8:
					Main.Push(8);
					break;
				case Op.Num9:
					Main.Push(9);
					break;
				case Op.Num10:
					Main.Push(10);
					break;
				case Op.Num11:
					Main.Push(11);
					break;
				case Op.Num12:
					Main.Push(12);
					break;
				case Op.Num13:
					Main.Push(13);
					break;
				case Op.Num14:
					Main.Push(14);
					break;
				case Op.Num15:
					Main.Push(15);
					break;
				case Op.Num16:
					Main.Push(16);
					break;

				case Op.Nop:
					break;
				case Op.If:
					throw new NotImplementedException();
				case Op.NotIf:
					throw new NotImplementedException();
				case Op.Else:
					throw new NotImplementedException();
				case Op.EndIf:
					throw new NotImplementedException();
				case Op.Verify:
					if(!Main.PopTruth())
						throw new Exception();
					break;
				case Op.Return:
					throw new Exception();

				case Op.ToAltStack:
					Alt.Push(Main.Pop());
					break;
				case Op.FromAltStack:
					Main.Push(Alt.Pop());
					break;
				case Op.IfDup:
					if(Main.IsTrue)
						Main.Push(Main[0]);
					break;
				case Op.Depth:
					Main.Push(Main.Count);
					break;
				case Op.Drop:
					Main.Pop();
					break;
				case Op.Dup:
					Main.Push(Main[0]);
					break;
				case Op.Nip:
					Main.RemoveAt(1);
					break;
				case Op.Over:
					Main.Push(Main[1]);
					break;
				case Op.Pick:
					Main.Push(Main[Main.PopInt()]);
					break;
				case Op.Roll:
					int index = Main.PopInt();
					byte[] roll = Main[index];
					Main.RemoveAt(index);
					Main.Push(roll);
					break;
				case Op.Rot:
					byte[][] rot = Main.Pop(3);
					Main.Push(new byte[][] { rot[1], rot[2], rot[0] });
					break;
				case Op.Swap:
					byte[][] swap = Main.Pop(2);
					Main.Push(new byte[][] { swap[1], swap[0] });
					break;
				case Op.Tuck:
					byte[][] tuck = Main.Pop(2);
					Main.Push(new byte[][] { tuck[1], tuck[0], tuck[1] });
					break;
				case Op.Drop2:
					Main.Pop(2);
					break;
				case Op.Dup2:
					Main.Push(new byte[][] { Main[0], Main[1] });
					break;
				case Op.Dup3:
					Main.Push(new byte[][] { Main[0], Main[1], Main[2] });
					break;
				case Op.Over2:
					Main.Push(new byte[][] { Main[2], Main[3] });
					break;
				case Op.Rot2:
					byte[][] rot2 = Main.Pop(6);
					Main.Push(new byte[][] { rot2[2], rot2[3], rot2[4], rot2[5], rot2[0], rot2[1] });
					break;
				case Op.Swap2:
					byte[][] swap2 = Main.Pop(4);
					Main.Push(new byte[][] { swap2[2], swap2[3], swap2[0], swap2[1] });
					break;

				case Op.Size:
					throw new NotImplementedException();

				case Op.Equal:
					Main.Push(Main[0] == Main[1]);
					break;
				case Op.EqualVerify:
					Main.Push(Main[0] == Main[1]);
					goto case Op.Verify;
				case Op.Add1:
					Main.Push(Main.PopInt() + 1);
					break;
				case Op.Sub1:
					Main.Push(Main.PopInt() - 1);
					break;
				case Op.Negate:
					Main.Push(-Main.PopInt());
					break;
				case Op.Abs:
					Main.Push(Math.Abs(Main.PopInt()));
					break;
				case Op.Not:
					int toFlip = Main.PopInt();
					if(toFlip == 0)
						Main.Push(1);
					else
						Main.Push(0);
					break;
				case Op.NotEqual0:
					if(Main.PopInt() == 0)
						Main.Push(0);
					else
						Main.Push(1);
					break;
				case Op.Add:
					Main.Push(Main.PopInt() + Main.PopInt());
					break;
				case Op.Sub:
					Main.Push(Main.PopInt() - Main.PopInt());
					break;
				case Op.BoolAnd:
					Main.Push(Main.PopBool() && Main.PopBool());
					break;
				case Op.BoolOr:
					Main.Push(Main.PopBool() || Main.PopBool());
					break;
				case Op.NumEqual:
					Main.Push(Main.PopInt() == Main.PopInt());
					break;
				case Op.NumEqualVerify:
					Main.Push(Main.PopInt() == Main.PopInt());
					goto case Op.Verify;
				case Op.NumNotEqual:
					Main.Push(Main.PopInt() != Main.PopInt());
					break;
				case Op.LessThan:
					Main.Push(Main.PopInt() < Main.PopInt());
					break;
				case Op.GreaterThan:
					Main.Push(Main.PopInt() > Main.PopInt());
					break;
				case Op.LessThanOrEqual:
					Main.Push(Main.PopInt() <= Main.PopInt());
					break;
				case Op.GreaterThanOrEqual:
					Main.Push(Main.PopInt() >= Main.PopInt());
					break;
				case Op.Min:
					Main.Push(Math.Min(Main.PopInt(), Main.PopInt()));
					break;
				case Op.Max:
					Main.Push(Math.Max(Main.PopInt(), Main.PopInt()));
					break;
				case Op.Within:
					int x = Main.PopInt();
					int min = Main.PopInt();
					int max = Main.PopInt();
					Main.Push(min <= x && x < max);
					break;

				case Op.RIPEMD160:
					Main.Push(RIPEMD160.Create().ComputeHash(Main.Pop()));
					break;
				case Op.SHA1:
					Main.Push(SHA1.Create().ComputeHash(Main.Pop()));
					break;
				case Op.SHA256:
					Main.Push(SHA256.Create().ComputeHash(Main.Pop()));
					break;
				case Op.CodeSeparator:
					throw new NotImplementedException();
				case Op.CheckSig:
					throw new NotImplementedException();
				case Op.CheckSigVerify:
					throw new NotImplementedException();
				case Op.CheckMultiSig:
					throw new NotImplementedException();
				case Op.CheckMultiSigVerify:
					throw new NotImplementedException();

				case Op.Reserved:
					throw new Exception();
				case Op.Ver:
					throw new Exception();
				case Op.VerIf:
					throw new Exception();
				case Op.VerNotIf:
					throw new Exception();
				case Op.Reserved1:
					throw new Exception();
				case Op.Reserved2:
					throw new Exception();
				case Op.Nop1:
					break;
				case Op.Nop2:
					break;
				case Op.Nop3:
					break;
				case Op.Nop4:
					break;
				case Op.Nop5:
					break;
				case Op.Nop6:
					break;
				case Op.Nop7:
					break;
				case Op.Nop8:
					break;
				case Op.Nop9:
					break;
				case Op.Nop10:
					break;

				default:
					throw new Exception(String.Format("{0} is an invalid opcode", next));
				}
			}
		}

		public void Execute() { // TODO: Async
			while(Execution.Count > 0) {
				Step();
			}
		}
	}
}

