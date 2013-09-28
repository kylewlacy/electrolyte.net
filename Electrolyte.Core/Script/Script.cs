using System;
using System.Linq;
using System.Collections.Generic;
using Electrolyte.Portable.Cryptography;
using Electrolyte.Messages;
using Electrolyte.Portable;
using Electrolyte.Helpers;

namespace Electrolyte {
	public partial class Script {
		public Transaction Transaction;
		public int InputIndex;

		public DataStack Main, Alt;
		Stack<byte> _start;
		Stack<byte> _execution;

		public Stack<byte> Execution {
			get { return _execution; }
			set {
				_execution = value;
				var items = new byte[_execution.Items.Count];
				_execution.Items.ToArray().CopyTo(items, 0);
				_start = new Stack<byte>(items.Reverse().ToArray());
			}
		}

		int lastSeparatorIndex = 0;
		public Stack<byte> SubScript {
			get {
				return new Stack<byte>(ArrayHelpers.SubArray(_start.ToArray(), lastSeparatorIndex));
			}
		}

		public Stack<bool> RunBranch = new Stack<bool>(new bool[] { true });

		public Script() : this(new byte[] { }) { }

		public Script(byte[] script) {
			Execution = new Stack<byte>(script);
			Main = new DataStack();
			Alt = new DataStack();
		}

		// TODO: Remove this in favor of `Script.Create`
		public Script(params object[] script) {
			var bytes = new List<byte>();
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

		public static Script FromString(string script) {
			List<byte> bytes = new List<byte>();

			foreach(string data in script.Split(' ')) {
				if(Opcodes.IsOpcode(data))
					bytes.Add((byte)Opcodes.Strings[data]);
				else
					bytes.AddRange(Opcodes.Pack(data));
			}

			return new Script(bytes.ToArray());
		}

		public void Step(int count) {
			for(int i = 0; i < count; i++)
				Step();
		}

		public void Step() {
			byte next = Execution.Pop();
			bool branchIsRunning = RunBranch.Items.All(b => b);

			if(branchIsRunning) {
				if(Opcodes.IsFastPush(next)) {
					Main.Push(Execution.Pop(next));
				}
				else {
					switch((Op)next) {
					case Op.False:
						Main.Push(false);
						break;
					case Op.PushData1:
						int i = new SignedInt(Execution.Pop()).Value;
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
						Main.Push(new byte[][] { rot[0], rot[2], rot[1] });
						break;
					case Op.Swap:
						byte[][] swap = Main.Pop(2);
						Main.Push(new byte[][] { swap[0], swap[1] });
						break;
					case Op.Tuck:
						byte[][] tuck = Main.Pop(2);
						Main.Push(new byte[][] { tuck[0], tuck[1], tuck[0] });
						break;
					case Op.Drop2:
						Main.Pop(2);
						break;
					case Op.Dup2:
						Main.Push(new byte[][] { Main[1], Main[0] });
						break;
					case Op.Dup3:
						Main.Push(new byte[][] { Main[2], Main[1], Main[0] });
						break;
					case Op.Over2:
						Main.Push(new byte[][] { Main[3], Main[2] });
						break;
					case Op.Rot2:
						byte[][] rot2 = Main.Pop(6);
						Main.Push(new byte[][] { rot2[1], rot2[0], rot2[5], rot2[4], rot2[3], rot2[2] });
						break;
					case Op.Swap2:
						byte[][] swap2 = Main.Pop(4);
						Main.Push(new byte[][] { swap2[2], swap2[3], swap2[0], swap2[1] });
						break;

					case Op.Size:
						throw new NotImplementedException();

					case Op.Equal:
						Main.Push(Main[0].SequenceEqual(Main[1]));
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
						Main.Push(Main.PopInt() == 0 ? 1 : 0);
						break;
					case Op.NotEqual0:
						Main.Push(Main.PopInt() == 0 ? 0 : 1);
						break;
					case Op.Add:
						Main.Push(Main.PopInt() + Main.PopInt());
						break;
					case Op.Sub:
						int b = Main.PopInt();
						Main.Push(Main.PopInt() - b);
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
						int max = Main.PopInt();
						int min = Main.PopInt();
						int x = Main.PopInt();
						Main.Push(min <= x && x < max);
						break;

					case Op.RIPEMD160:
						Main.Push(RIPEMD160.Hash(Main.Pop()));
						break;
					case Op.SHA1:
						Main.Push(SHA1.Hash(Main.Pop()));
						break;
					case Op.SHA256:
						Main.Push(SHA256.Hash(Main.Pop()));
						break;
					case Op.Hash160:
						Main.Push(Digest.Hash<RIPEMD160, SHA256>(Main.Pop()));
						break;
					case Op.Hash256:
						Main.Push(Digest.Hash<SHA256, SHA256>(Main.Pop()));
						break;
					case Op.CodeSeparator:
						lastSeparatorIndex = _start.Count - Execution.Count;
						break;
					case Op.CheckSig:
//						Main.Push(Transaction.SigIsValid(Main.Pop(), Main.Pop(), SubScript, InputIndex));
						throw new NotImplementedException();
					case Op.CheckSigVerify:
						throw new NotImplementedException();
					case Op.CheckMultiSig:
						throw new NotImplementedException();
					case Op.CheckMultiSigVerify:
						throw new NotImplementedException();

					case Op.Reserved:
					case Op.Ver:
					case Op.Reserved1:
					case Op.Reserved2:
						throw new Exception(String.Format("Invalid script! (opcode {0} used)", (Op)next));

						
					case Op.If:
					case Op.NotIf:
					case Op.Else:
					case Op.EndIf:
						goto case Op.Nop;
					
					case Op.Nop:
					case Op.Nop1:
					case Op.Nop2:
					case Op.Nop3:
					case Op.Nop4:
					case Op.Nop5:
					case Op.Nop6:
					case Op.Nop7:
					case Op.Nop8:
					case Op.Nop9:
					case Op.Nop10:
						break;

					default:
						throw new Exception(String.Format("{0} is an invalid opcode", next));
					}
				}
			}

			switch((Op)next) {
			case Op.If:
				RunBranch.Push(branchIsRunning && Main.PopBool());
				break;
			case Op.NotIf:
				RunBranch.Push(branchIsRunning && !Main.PopBool());
				break;
			case Op.Else:
				RunBranch.Push(!RunBranch.Pop() && !branchIsRunning);
				break;
			case Op.EndIf:
				RunBranch.Pop();
				break;
			case Op.VerIf:
			case Op.VerNotIf:
				throw new Exception(String.Format("Invalid script! (opcode {0} included)", (Op)next));
			}
		}

		public void Execute() { // TODO: Async
			while(Execution.Count > 0)
				Step();
		}

		public override string ToString() {
			var dataStack = new Stack<byte>(_start);
			var unpacked = new List<string>();

			while(dataStack.Count > 0) {
				byte next = dataStack.Pop();

				if(Opcodes.IsPush(next)) {
					int toPush = 0;

					if(Opcodes.IsFastPush(next))
						toPush = next;
					else if(next == (byte)Op.PushData1)
						toPush = new SignedInt(dataStack.Pop()).Value;
					else if(next == (byte)Op.PushData2)
						toPush = new SignedInt(dataStack.Pop(2)).Value;
					else if(next == (byte)Op.PushData4)
						toPush = new SignedInt(dataStack.Pop(4)).Value;

					unpacked.Add(BinaryHelpers.ByteArrayToHex(dataStack.Pop(toPush)).ToUpper());
				}

				else {
					unpacked.Add(Opcodes.Strings.FirstOrDefault(x => x.Value == (Op)next).Key ?? next.ToString());
				}
			}

			return String.Join(" ", unpacked);
		}

		public static Script Create(params object[] script) {
			var bytes = new List<byte>();
			foreach(var o in script) {
				if(o is byte[])
					bytes.AddRange(Opcodes.Pack((byte[])o));
				// TODO: Do either of these need to be flipped? (Write some tests)
				//				else if(o is int)
				//						bytes.AddRange(new SignedInt((int)o).ToByteArray());
				//				else if(o is SignedInt)
				//					bytes.AddRange(((SignedInt)o).ToByteArray());
				else
					bytes.Add((byte)o);
			}

			return new Script(bytes.ToArray());
		}
	}
}

