using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace TetraPak.XP
{
    /// <summary>
    ///   Provides convenient helper methods or working with collections.
    /// </summary>
    public static class CollectionHelper
    {
        public static T[] Join<T>(this T[]? self, T[]? other)
        {
            self ??= Array.Empty<T>();
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

        public static T[] MakeArrayOf<T>(int count, CollectionItemActivator<T>? activator = null)
        {
            var array = new T[count]; 
            for (var i = 0; i < count; i++)
            {
                if (activator is { })
                {
                    array[i] = activator.Invoke(i);
                }
                else
                {
                    array[i] = Activator.CreateInstance<T>();
                }
            }

            return array;
        }
        
        public static T[] MakeArrayOf<T>(T value, int count) => MakeArrayOf(count, _ => value);

        /// <summary>
        ///   Constructs and returns a <see cref="IList"/> of items of a specified type.
        /// </summary>
        /// <param name="itemType">
        ///   The type of items to be contained by the list.
        /// </param>
        /// <param name="count">
        ///   The number of items to be created.
        /// </param>
        /// <param name="itemFactory">
        ///   A callback handler charged with constructing and returning the items for the constructed list.
        /// </param>
        /// <returns>
        ///   A <see cref="IList"/> object that contains items constructed by <see cref="itemFactory"/>.
        /// </returns>
        /// <exception cref="TargetInvocationException">
        ///   <see cref="Activator.CreateInstance(string,string)"/> failed to construct and activate
        ///   an <see cref="IList"/> instance.
        /// </exception>
        /// <seealso cref=" MakeListOf"/>
        public static IList MakeListOf(Type itemType, int count, CollectionItemActivator? itemFactory = null)
        {
            var listType = itemType.MakeGenericType(itemType);
            if (Activator.CreateInstance(listType) is not IList list)
                throw new TargetInvocationException($"Failed to activate list of {itemType}", null);
            
            for (var i = 0; i < count; i++)
            {
                var item = itemFactory?.Invoke(i) ?? Activator.CreateInstance(itemType);
                list.Add(item);
            }

            return list;
        }

        public static List<T> MakeListOf<T>(T value, int count) => MakeListOf(count, _ => value);
        
        public static List<T> MakeListOf<T>(int count, CollectionItemActivator<T>? activator = null)
        {
            var list = new List<T>(); 
            for (var i = 0; i < count; i++)
            {
                list.Add(activator is { } ? activator.Invoke(i) : Activator.CreateInstance<T>());
            }

            return list;
        }

    }

    public delegate T CollectionItemActivator<out T>(int index);
    
    public delegate object CollectionItemActivator(int index);
}