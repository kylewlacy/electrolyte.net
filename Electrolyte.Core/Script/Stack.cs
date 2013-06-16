using System;
using System.Linq;
using System.Collections.Generic;
using Electrolyte.Primitives;

namespace Electrolyte {
	public partial class Script {
		public class Stack<T> {
			public List<T> Items;

			public Stack() {
				Items = new List<T>();
			}

			public Stack(T[] stack) {
				Items = new List<T>(stack);
			}

			public Stack(Stack<T> stack) : this(stack.ToArray()) { }

			public T this[int i] {
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

			public void Push(T data) {
				Items.Add(data);
			}

			public void Push(T[] data) {
				Items.AddRange(data);
			}

			public T Pop() {
				T data = this[0];
				RemoveAt(0);
				return data;
			}

			public T[] Pop(int count) {
				List<T> data = new List<T>();
				for(int i = 0; i < count; i++) { data.Add(Pop()); }
				return data.ToArray();
			}

			public T[] ToArray() {
				return Items.ToArray();
			}

			public List<T> ToList() {
				return Items;
			}
		}

		public class DataStack : Electrolyte.Script.Stack<byte[]> {
			public bool IsTrue {
				get { return DataAsInteger() != 0; }
			}

			public DataStack() : base() { }
			public DataStack(byte[][] data) : base(data) { }
			public DataStack(DataStack stack) : base(stack) { }

			public Int32 DataAsInteger() {
				return DataAsInteger(0);
			}

			public Int32 DataAsInteger(int index) {
				return new SignedInt(this[index]).Value;
			}

			public void Push(int data) {
				Push(new SignedInt(data).ToByteArray());
			}

			public void Push(bool data) {
				Push(data ? 1 : 0);
			}

			public Int32 PopInt() {
				return new SignedInt(Pop().Reverse().ToArray()).Value;
			}

			public bool PopBool() {
				return PopInt() != 0;
			}

			public bool PopTruth() {
				return (IsTrue ? PopBool() : false);
			}
		}
	}
}

