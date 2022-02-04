using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace TetraPak.XP
{
    public static class Collection
    {
        public static T[] Join<T>(this T[]? self, T[]? other)
        {
            self ??= System.Array.Empty<T>();
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

        public static T[] ArrayOf<T>(int count, CollectionItemActivator<T>? activator = null)
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
        
        public static T[] ArrayOf<T>(T value, int count) => ArrayOf(count, _ => value);

        public static IList ListOf(Type itemType, object value, int count) 
            => ListOf(itemType, count, _ => value);

        public static IList ListOf(Type itemType, int count, CollectionItemActivator? activator = null)
        {
            var listType = itemType.MakeGenericType(itemType);
            var list = (IList) Activator.CreateInstance(listType);
            if (list is null)
                throw new TargetInvocationException($"Failed to activate list of {itemType}", null);
            
            for (var i = 0; i < count; i++)
            {
                var item = activator?.Invoke(i) ?? Activator.CreateInstance(itemType);
                list.Add(item);
            }

            return list;
        }

        public static List<T> ListOf<T>(T value, int count) => ListOf<T>(count, _ => value);
        
        public static List<T> ListOf<T>(int count, CollectionItemActivator<T>? activator = null)
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