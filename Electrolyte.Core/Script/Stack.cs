using System;
using System.Collections.Generic;

namespace Electrolyte {
	public partial class Script {
		public class Stack {
			public List<byte> Items;

			public bool IsTrue {
				get { return this[0] != (byte)Op.False }
			}

			public Stack() {
				Items = new List<byte>();
			}

			public Stack(byte[] stack) {
				Items = new List<byte>(stack);
			}

			public byte this[int i] {
				get {
					if(i >= 0)
						return Items[Items.Count - 1 - i];
					else
						return Items[-i];
				}
			}

			public int Count {
				get { return Items.Count; }
			}

			public void RemoveAt(int i) {
				Items.RemoveAt(Items.Count - 1 - i);
			}

			public void Push(byte data) {
				Items.Add(data);
			}

			public void Push(byte[] data) {
				Items.AddRange(data);
			}

			public byte Pop() {
				byte data = this[0];
				RemoveAt(0);
				return data;
			}

			public byte[] Pop(int count) {
				List<byte> data = new List<byte>();
				for(int i = 0; i < count; i++) { data.Add(Pop()); }
				return data.ToArray();
			}

			public byte[] ToArray() {
				return Items.ToArray();
			}

			public List<byte> ToList() {
				return Items;
			}
		}
	}
}

