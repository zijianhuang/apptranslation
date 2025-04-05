namespace Fonlow.Translate
{
	public static class ArraySplitter
	{
		/// <summary>
		/// Splits an array into several smaller arrays.
		/// </summary>
		/// <typeparam name="T">The type of the array.</typeparam>
		/// <param name="array">The array to split.</param>
		/// <param name="size">The size of the smaller arrays.</param>
		/// <returns>An array containing smaller arrays.</returns>
		public static IEnumerable<IList<T>> SplitLists<T>(this IList<T> array, int size)
		{
			for (var i = 0; i < (float)array.Count / size; i++)
			{
				yield return array.Skip(i * size).Take(size).ToList();
			}
		}
		//thanks to https://stackoverflow.com/questions/18986129/how-can-i-split-an-array-into-n-parts

		public static IEnumerable<IEnumerable<T>> Split<T>(this T[] array, int size)
		{
			for (var i = 0; i < (float)array.Length / size; i++)
			{
				yield return array.Skip(i * size).Take(size);
			}
		}

	}
}
