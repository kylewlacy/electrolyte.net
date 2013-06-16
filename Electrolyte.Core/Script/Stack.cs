using System;
using System.Collections.Generic;

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
	}
}

