using System;
using System.Linq;
using System.Collections.Generic;
using Electrolyte.Helpers;

namespace Electrolyte {
	public enum Op : byte {
		False               = 0x00,
		PushData1           = 0x4C,
		PushData2           = 0x4D,
		PushData4           = 0x4E,
		Negate1             = 0x4F,
		True                = 0x51,
		Num2                = 0x52,
		Num3                = 0x53,
		Num4                = 0x54,
		Num5                = 0x55,
		Num6                = 0x56,
		Num7                = 0x57,
		Num8                = 0x58,
		Num9                = 0x59,
		Num10               = 0x5A,
		Num11               = 0x5B,
		Num12               = 0x5C,
		Num13               = 0x5D,
		Num14               = 0x5E,
		Num15               = 0x5F,
		Num16               = 0x60,

		Nop                 = 0x61,
		If                  = 0x63,
		NotIf               = 0x64,
		Else                = 0x67,
		EndIf               = 0x68,
		Verify              = 0x69,
		Return              = 0x6A,

		ToAltStack          = 0x6B,
		FromAltStack        = 0x6C,
		Drop2               = 0x6D,
		Dup2                = 0x6E,
		Dup3                = 0x6F,
		Over2               = 0x70,
		Rot2                = 0x71,
		Swap2               = 0x72,
		IfDup               = 0x73,
		Depth               = 0x74,
		Drop                = 0x75,
		Dup                 = 0x76,
		Nip                 = 0x77,
		Over                = 0x78,
		Pick                = 0x79,
		Roll                = 0x7A,
		Rot                 = 0x7B,
		Swap                = 0x7C,
		Tuck                = 0x7D,

		Cat                 = 0x7E,
		SubStr              = 0x7F,
		Left                = 0x80,
		Right               = 0x81,
		Size                = 0x82,

		Invert              = 0x83,
		And                 = 0x84,
		Or                  = 0x85,
		Xor                 = 0x86,
		Equal               = 0x87,
		EqualVerify         = 0x88,

		Add1                = 0x8B,
		Sub1                = 0x8C,
		Mul2                = 0x8D,
		Div2                = 0x8E,
		Negate              = 0x8F,
		Abs                 = 0x90,
		Not                 = 0x91,
		NotEqual0           = 0x92,
		Add                 = 0x93,
		Sub                 = 0x94,
		Mul                 = 0x95,
		Div                 = 0x96,
		Mod                 = 0x97,
		LShift              = 0x98,
		RShift              = 0x99,
		BoolAnd             = 0x9A,
		BoolOr              = 0x9B,
		NumEqual            = 0x9C,
		NumEqualVerify      = 0x9D,
		NumNotEqual         = 0x9E,
		LessThan            = 0x9F,
		GreaterThan         = 0xA0,
		LessThanOrEqual     = 0xA1,
		GreaterThanOrEqual  = 0xA2,
		Min                 = 0xA3,
		Max                 = 0xA4,
		Within              = 0xA5,

		RIPEMD160           = 0xA6,
		SHA1                = 0xA7,
		SHA256              = 0xA8,
		Hash160             = 0xA9,
		Hash256             = 0xAA,
		CodeSeparator       = 0xAB,
		CheckSig            = 0xAC,
		CheckSigVerify      = 0xAD,
		CheckMultiSig       = 0xAE,
		CheckMultiSigVerify = 0xAF,

		PubKeyHash          = 0xFD,
		PubKey              = 0xFE,
		InvalidOpCode       = 0xFF,

		Reserved            = 0x50,
		Ver                 = 0x62,
		VerIf               = 0x65,
		VerNotIf            = 0x66,
		Reserved1           = 0x89,
		Reserved2           = 0x8A,
		Nop1                = 0xB0,
		Nop2                = 0xB1,
		Nop3                = 0xB2,
		Nop4                = 0xB3,
		Nop5                = 0xB4,
		Nop6                = 0xB5,
		Nop7                = 0xB6,
		Nop8                = 0xB7,
		Nop9                = 0xB8,
		Nop10               = 0xB9
	}

	public static class Opcodes {
		static readonly Dictionary<string, Op> _strings = new Dictionary<string, Op> {
			{"OP_FALSE", Op.False},
			{"OP_TRUE", Op.True},
			{"OP_1NEGATE", Op.Negate1},
			{"OP_0", Op.False},
			{"OP_1", Op.True},
			{"OP_2", Op.Num2},
			{"OP_3", Op.Num3},
			{"OP_4", Op.Num4},
			{"OP_5", Op.Num5},
			{"OP_6", Op.Num6},
			{"OP_7", Op.Num7},
			{"OP_8", Op.Num8},
			{"OP_9", Op.Num9},
			{"OP_10", Op.Num10},
			{"OP_11", Op.Num11},
			{"OP_12", Op.Num12},
			{"OP_13", Op.Num13},
			{"OP_14", Op.Num14},
			{"OP_15", Op.Num15},
			{"OP_16", Op.Num16},
			{"OP_2DROP", Op.Drop2},
			{"OP_2DUP", Op.Dup2},
			{"OP_3DUP", Op.Dup3},
			{"OP_2OVER", Op.Over2},
			{"OP_2ROT", Op.Rot2},
			{"OP_2SWAP", Op.Swap2}
		};
		public static Dictionary<string, Op> Strings {
			get {
				Array values = Enum.GetValues(typeof(Op));
				if(_strings.Count < values.Length) {
					foreach(Op op in values) {
						if(!_strings.ContainsValue(op))
							_strings.Add(String.Format("OP_{0}", String.Join("", op.ToString().ToCharArray().Select(char.ToUpper))), op);
					}
				}

				return _strings;
			}
		}

		public static bool IsOpcode(string str) {
			return Strings.ContainsKey(str);
		}

		public static bool IsPush(Op op) {
			return IsFastPush(op) || op == Op.PushData1 || op == Op.PushData2 || op == Op.PushData4;
		}

		public static bool IsPush(byte op) {
			return IsPush((Op)op);
		}

		public static bool IsFastPush(Op op) {
			return IsFastPush((byte)op);
		}

		public static bool IsFastPush(byte op) {
			return 1 <= op && op <= 75;
		}

		public static byte[] Pack(params byte[] data) {
			byte[] header;
			if(data.Length <= 75)
				header = new byte[] { (byte)data.Length };
			else if(data.Length <= Byte.MaxValue)
				header = new byte[] { (byte)Op.PushData1, (byte)data.Length };
			else if(data.Length <= Int16.MaxValue)
				header = ArrayHelpers.ConcatArrays(new byte[] { (byte)Op.PushData2 }, BitConverter.GetBytes((Int16)data.Length));
			else
				header = ArrayHelpers.ConcatArrays(new byte[] { (byte)Op.PushData4 }, BitConverter.GetBytes((Int32)data.Length));

			return ArrayHelpers.ConcatArrays(header, data);
		}

		public static byte[] Pack(string data) {
			if(data.Substring(0, 2) == "0x")
				data = data.Substring(2, data.Length - 2);

			var bytes = new byte[data.Length / 2];
			for(int i = 0; i < bytes.Length; i++)
				bytes[i] = Convert.ToByte(data.Substring(i*2, 2), 16);

			return Pack(bytes);
		}
	}
}

