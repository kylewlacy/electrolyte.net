// From https://gist.github.com/CodesInChaos/3175971
using System;
using System.Linq;

namespace Electrolyte.Helpers {
	public class ArrayHelpers {
		public static T[] ConcatArrays<T>(params T[][] arrays) {
			var result = new T[arrays.Sum(arr => arr.Length)];
			int offset = 0;
			foreach(var arr in arrays) {
				Buffer.BlockCopy(arr, 0, result, offset, arr.Length);
				offset += arr.Length;
			}
			return result;
		}

		public static T[] ConcatArrays<T>(T[] arr1, T[] arr2) {
			var result = new T[arr1.Length + arr2.Length];
			Buffer.BlockCopy(arr1, 0, result, 0, arr1.Length);
			Buffer.BlockCopy(arr2, 0, result, arr1.Length, arr2.Length);
			return result;
		}

		public static T[] SubArray<T>(T[] arr, int start, int length) {
			if(start + length > arr.Length)
				throw new ArgumentException();

			var result = new T[length];
			Buffer.BlockCopy(arr, start, result, 0, length);
			return result;
		}

		public static T[] SubArray<T>(T[] arr, int start) {
			return SubArray(arr, start, arr.Length - start);
		}
	}
}