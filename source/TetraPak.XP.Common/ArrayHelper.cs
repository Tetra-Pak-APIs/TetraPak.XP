using System;
using System.Collections;
using System.Collections.Generic;

namespace TetraPak
{
    public static class ArrayHelper
    {
        public static T[] Join<T>(this T[] self, T[] other)
        {
            self ??= new T[0];
            if (other is null || other.Length == 0)
                return self;

            var result = new T[self.Length + other.Length];
            self.CopyTo(result, 0);
            other.CopyTo(result, self.Length);
            return result;
        }

        public static object[] EnumerableToArray(this IEnumerable enumerable)
        {
            var list = new List<object>();
            var enumerator = enumerable.GetEnumerator();
            while (enumerator.MoveNext())
            {
                list.Add(enumerator.Current!);
            }
        
            return list.ToArray();
        }
        
        public static T[] EnumerableToArray<T>(this IEnumerable enumerable, bool skipTypeIncompatible = false)
        {
            var list = new List<T>();
            var enumerator = enumerable.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current is T tValue)
                {
                    list.Add(tValue);
                    continue;
                }

                if (!skipTypeIncompatible)
                    throw new InvalidCastException(
                        $"Failed to create array from enumerable. One or more items cannot be cast as {typeof(T)}");
            }

            return list.ToArray();
        }

    }
}