using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#nullable enable

namespace TetraPak.XP
{
    public static class TypeHelper
    {

        public static bool Is(
            this Type? self, 
            Type? type, 
            bool laxEnumerationComparison = true, 
            bool includeImplicitOperatorOverloads = false)
        {
            if (self is null || type is null)
                return false;

            if (type == self)
                return true;

            if (type.IsInterface)
                return self.ImplementsInterface(type);
            
            if (type.IsAssignableFrom(self) || self.IsAssignableFrom(type))
                return true;

            if (includeImplicitOperatorOverloads && type == typeof(string) && self.IsOverloadingImplicitOperator<string>())
                return true;

            if (!self.IsCollection())
                return false;

            if (!type.IsCollection() || !laxEnumerationComparison) 
                return false;

            var type1 = self.GetCollectionElementType();
            var type2 = type.GetCollectionElementType();
            return type1?.Is(type2) ?? false;
        }
        
        public static bool ImplementsInterface<T>(this Type? self) 
            => self.ImplementsInterface(typeof(T));

        public static bool ImplementsInterface(this Type? self, Type @interface) 
            => self?.GetInterface(@interface.FullName ?? string.Empty) != null;

        public static bool IsNullable(this Type self)
        {
            return Nullable.GetUnderlyingType(self) is { };
        }
        
        public static bool IsNullable<T>(this T self)
        {
            return self is null || self.GetType().IsNullable();
        }

        public static bool IsNumeric(this object? self) => self?.GetType().IsNumeric() ?? false;
        
        public static bool IsNumeric(this Type self)
        {
            switch (Type.GetTypeCode(self))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Decimal:
                case TypeCode.Single:
                case TypeCode.Double:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsZero(this object? self, bool nullIsZero = false)
        {
            if (self is null)
                return nullIsZero;

            var typeCode = Type.GetTypeCode(self.GetType());
            return typeCode switch
            {
                TypeCode.Byte => (byte) self == 0,
                TypeCode.SByte => (sbyte) self == 0,
                TypeCode.Int16 => (short) self == 0,
                TypeCode.Int32 => (int) self == 0,
                TypeCode.Int64 => (short) self == 0,
                TypeCode.UInt16 => (ushort) self == 0u,
                TypeCode.UInt32 => (uint) self == 0u,
                TypeCode.UInt64 => (ulong) self == 0u,
                TypeCode.Decimal => (decimal) self == 0,
                TypeCode.Single => (float) self == 0f,
                TypeCode.Double => (double) self == 0d,
                TypeCode.Empty => false,
                TypeCode.Object => false,
                TypeCode.DBNull => true,
                TypeCode.Boolean => (bool) self,
                TypeCode.Char => (char)self == (char)0,
                TypeCode.DateTime => (DateTime)self == default,
                TypeCode.String => string.IsNullOrWhiteSpace((string) self),
                _ => throw new NotSupportedException($"Unsupported type code: {typeCode}")
            };
        }

        public static bool IsGenericBase(this Type self, Type genericType, bool inherited = true)
        {
            if (!genericType.IsGenericType || genericType.GenericTypeArguments.Length != 0) 
                throw new ArgumentException($"Expected generic base type (no generic arguments) but found {genericType}");

            var targetGenericName = getGenericName(genericType);
            while (true)
            {
                if (!self.IsGenericType)
                {
                    if (inherited)
                        goto next;
                }

                var genericName = getGenericName(self);
                if (genericName == targetGenericName)
                    return true;

                next:

                if (!inherited || self.BaseType is null) 
                    return false;

                self = self.BaseType;
            }
        }

        public static bool TryGetGenericBase(
            this Type self,
            Type genericType, 
            /*[NotNullWhen(true)]*/ out Type? type, 
            bool inherited = true)
        {
            if (!genericType.IsGenericType || genericType.GenericTypeArguments.Length != 0) 
                throw new ArgumentException($"Expected generic base type (no generic arguments) but found {genericType}");

            var targetGenericName = getGenericName(genericType);
            while (true)
            {
                if (!self.IsGenericType)
                    goto next;

                var genericName = getGenericName(self);
                if (genericName == targetGenericName)
                {
                    type = self;
                    return true;
                }

                next:
                
                if (!inherited || self.BaseType is null)
                {
                    type = null;
                    return false;
                }

                self = self.BaseType;
            }
        }

        static string getGenericName(Type type)
        {
            return $"{type.Namespace}.{type.Name}";
        }

        public static bool IsCollection(this Type type, out Type? collectionType, out Type? itemType, bool treatStringAsUnary = true)
        {
            collectionType = null!;
            itemType = null;
            if (treatStringAsUnary && type == typeof(string))
                return false;
                
            if (!type.ImplementsInterface<IEnumerable>())
            {
                collectionType = null;
                return false;
            }

            collectionType = type;
            if (type.IsArray)
            {
                itemType = type.GetElementType();
                return true;
            }
            
            if (!type.IsGenericType)
            {
                itemType = typeof(object);
                return true;
            }
            
            itemType = type.GetGenericArguments()[0];
            return true;
        }

        /// <summary>
        ///   Examine an object and returns information on whether it is a collection. 
        /// </summary>
        /// <param name="self">
        ///   The examined object. 
        /// </param>
        /// <param name="itemType">
        ///   Passes back the collection item type on success; otherwise a <c>null</c> value.
        /// </param>
        /// <param name="items">
        ///   Passes back the collection items on success; otherwise a <c>null</c> value.
        /// </param>
        /// <param name="count">
        ///   On success; passes back the number of items found in the collection, or -1
        ///   (if too expensive to resolve and <paramref name="alwaysResolveCount"/> is not set).
        ///   On failure a value of zero (0) is passed back. 
        /// </param>
        /// <param name="alwaysResolveCount">
        ///   (optional; default=<c>false</c>)<br/>
        ///   When set, and the object is considered a collection, the number of items found will always be resolved,
        ///   even when performance might be negatively impacted. When not set the <paramref name="count"/> might
        ///   be passed back as a negative value to indicate the value was not resolved.
        /// </param>
        /// <param name="treatStringAsUnary">
        ///   (optional; default=<c>true</c>)<br/>
        ///   When set <see cref="string"/>s will be treated as unary values, and not as collections.
        /// </param>
        /// <returns>
        ///   <c>true</c> if <paramref name="self"/> is a collection; otherwise <c>false</c>.
        /// </returns>
        public static bool IsCollection(
            this object self, 
            out Type? itemType,
            out IEnumerable? items,
            out int count, 
            bool alwaysResolveCount = false, 
            bool treatStringAsUnary = true)
        {
            items = null;
            itemType = null;
            count = 0;
            switch (self)
            {
                case null:
                case string when treatStringAsUnary:
                    return false;
                
                case string s:
                {
                    itemType = typeof(char);
                    var charArray = s.ToCharArray();
                    items = charArray;
                    count = charArray.Length;
                    return true;
                }
            }

            if (!(self is IEnumerable enumerable))
                return false;
        
            var type = enumerable.GetType();
            if (type.IsGenericType)
            {
                var typeArgs = enumerable.GetType().GenericTypeArguments;
                items = enumerable;
                count = alwaysResolveCount
                    ? resolveCount(items) // costly, but client have requested the 'count' value to be resolved
                    : -1;
                if (enumerable is IDictionary)
                {
                    itemType = typeof(KeyValuePair<,>).MakeGenericType(typeArgs);
                    return true;
                }
                itemType = typeArgs[0];
                return typeArgs.Length == 1 && itemType.IsAssignableFrom(typeArgs[0]);
            }
        
            if (!(enumerable is Array array)) 
                return false;
            
            itemType = type.GetElementType();
            items = array;
            count = array.Length;
            return true;
            
        }

        static int resolveCount(IEnumerable enumerable)
        {
            var c = 0;
            var enumerator = enumerable.GetEnumerator();
            while (enumerator.MoveNext())
            {
                ++c;
            }

            return c;
        }

        public static bool IsCollection(this Type type)
        {
            return type.ImplementsInterface<IEnumerable>();
        }
        
        public static bool IsCollectionOf(
            this object? obj, 
            Type itemType,
            out IEnumerable? items, 
            bool treatStringAsUnary = true)
        {
            items = null;
            switch (obj)
            {
                case null:
                    return false;
                case string _:
                    return !treatStringAsUnary;
            }

            var type = obj.GetType();
            if (!typeof(IEnumerable).IsAssignableFrom(type))
                return false;

            if (type.IsGenericType)
            {
                items = (IEnumerable) obj;
                var testCollectionType = typeof(IEnumerable<>).MakeGenericType(itemType);
                if (testCollectionType.IsAssignableFrom(type))
                    return true;
                
                var typeArgs = type.GenericTypeArguments;
                return typeArgs.Length == 1 && itemType.IsAssignableFrom(typeArgs[0]);
            }

            if (!(obj is Array array)) 
                return false;

            if (type.GetArrayRank() != 1 || !itemType.IsAssignableFrom(type.GetElementType()))
                return false;
            
            items = array;
            return true;
        }
        
        /// <summary>
        ///   Examines an arbitrary object and returns information indicating whether it is a
        ///   collection of a specified type (<typeparamref name="T"/>). 
        /// </summary>
        /// <param name="obj">
        ///   The arbitrary object to examine. 
        /// </param>
        /// <param name="items">
        ///   Passes back the collection items if <paramref name="obj"/> is a collection; otherwise <c>null</c>. 
        /// </param>
        /// <param name="treatStringAsUnary">
        ///   (optionalM default=<c>true</c>)<br/>
        ///   Specifies whether to treat <see cref="string"/>s as unary values (not collections).
        /// </param>
        /// <typeparam name="T">
        ///   The expected (item) <see cref="Type"/> (criteria).
        /// </typeparam>
        /// <returns>
        ///   <c>true</c> if <paramref name="obj"/> was found to be a collection of type <typeparamref name="T"/>;
        ///   otherwise <c>false</c>.
        /// </returns>
        public static bool IsCollectionOf<T>(
            this object? obj,
            /*[NotNullWhen(true)]*/ out IEnumerable<T?>? items, 
            bool treatStringAsUnary = true)
        {
            if (!obj.IsCollectionOf(typeof(T), out var enumerable, treatStringAsUnary))
            {
                items = null;
                return false;
            }
                
            var list = new List<T?>();
            var enumerator = enumerable!.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var item = (T?) enumerator.Current;
                list.Add(item);
            }

            items = list;
            return true;
        }

        public static Type? GetCollectionElementType(this Type self)
        {
            if (!self.IsCollection())
                throw new ArgumentException($"Cannot get item type from non-collection type {self}");

            if (self.IsArray)
                return self.GetElementType();

            if (self.IsGenericBase(typeof(IEnumerable<>)))
                return self.GetGenericArguments()[0];

            return typeof(object);
        }
        
        public static bool TryGetCollectionItemType(this Type? collectionType, /*[NotNullWhen(true)]*/ out Type? iType)
        {
            while (true)
            {
                if (collectionType is null)
                {
                    iType = null;
                    return false;
                }

                if (!collectionType.IsGenericType)
                {
                    collectionType = collectionType.BaseType;
                    continue;
                }

                iType = collectionType.GetGenericArguments()[0];
                return true;
            }
        }
        
        

        public static IEnumerable<T> WithInserted<T>(this IEnumerable<T> collection, int index, T item)
        {
            var list = collection.ToList();
            list.Insert(index, item);
            return list;
        }

         /// <summary>
         ///   Gets a value specifying whether the <seealso cref="Type"/> declares an
         ///   overloaded implicit type method.
         /// </summary>
         /// <typeparam name="T">
         ///   The overloaded <seealso cref="Type"/>.
         /// </typeparam>
         /// <param name="self">
         ///   The <see cref="Type"/> declaring the requested implicit overloaded type method.
         /// </param>
         /// <returns>
         ///   <c>true</c> if the type declares an implicit overloaded type method; otherwise <c>false</c>.
         /// </returns>
         /// <seealso cref="GetOverloadingImplicitOperator"/>
         public static bool IsOverloadingImplicitOperator<T>(this Type self) => self.GetOverloadingImplicitOperator(typeof(T)) != null;

         /// <summary>
         ///   Attempts getting an implicit overload operator type method.
         /// </summary>
         /// <param name="self">
         ///   The <see cref="Type"/> declaring the requested implicit overloaded type method.
         /// </param>
         /// <param name="type">
         ///   The overloaded <see cref="Type"/>.
         /// </param>
         /// <returns>
         ///   The <see cref="MethodInfo"/> if the requested implicit overloaded type method exists; otherwise <c>null</c>.
         /// </returns>
         /// <seealso cref="IsOverloadingImplicitOperator{T}"/>
         /// <seealso cref="IsOverloadingImplicitOperator{T}"/>
         public static MethodInfo? GetOverloadingImplicitOperator(this Type self, Type type)
         {
             return self.GetMethods().FirstOrDefault(overloadedImplicitOperator);
            
             bool overloadedImplicitOperator(MethodInfo m)
             {
                 if (m.Name != "op_Implicit" || !m.IsStatic || m.ReturnType != self)
                     return false;

                 var parameters = m.GetParameters();
                 if (parameters.Length != 1)
                     return false;

                 return parameters[0].ParameterType == type;
             }
         }
         
         public static TValue GetDefaultValue<TValue>() => (TValue) GetDefaultValue(typeof(TValue))!;

         public static object? GetDefaultValue(this Type type)
         {
             return type.IsValueType 
                 ? Activator.CreateInstance(type) 
                 : null;
         }

         /// <summary>
         ///   Examines a collection and returns a value to indicate it contains
         ///   at least one item that is also contained in another collection. 
         /// </summary>
         /// <param name="self">
         ///   The collection to be examined.
         /// </param>
         /// <param name="other">
         ///   A collection of items to look for.
         /// </param>
         /// <param name="comparer">
         ///   (optional)<br/>
         ///   Specifies how to compare the items.
         /// </param>
         /// <returns>
         ///   <c>true</c> if one (or more) item in <paramref name="other"/> is contained by
         ///   the collection.
         /// </returns>
         public static bool ContainsAny<T>(this IEnumerable<T> self, IEnumerable<T>? other, Func<T,T,bool>? comparer = null)
         {
             if (other is null)
                 return false;

             return comparer is {}
                 ? self.Any(s => other.Any(i => comparer(i, s))) 
                 : self.Any(s => other.Any(i => i?.Equals(s) ?? false));
         }

         /// <summary>
         ///   Examines a collection of <see cref="string"/> and returns a value to indicate it contains
         ///   at least one item that is also contained in another collection of <see cref="string"/>s. 
         /// </summary>
         /// <param name="self">
         ///   The collection of <see cref="string"/>s to be examined.
         /// </param>
         /// <param name="other">
         ///   A collection of strings to look for.
         /// </param>
         /// <param name="comparison">
         ///   (optional)<br/>
         ///   Specifies how to compare <see cref="string"/>s.
         /// </param>
         /// <returns>
         ///   <c>true</c> if one (or more) item in <paramref name="other"/> is contained by
         ///   the collection of <see cref="string"/>s.
         /// </returns>
         public static bool ContainsAny(this IEnumerable<string> self, IEnumerable<string> other, StringComparison? comparison = null)
         {
             return comparison.HasValue
                ? self.Any(s => other.Any(i => string.Equals(s, i, comparison.Value)))
                : self.Any(s => other.Any(i => string.Equals(s, i)));
         }

         public static int IndexOf<T>(this IEnumerable<T> self, T item) => self.IndexOf(i => i?.Equals(item) ?? false);
         
         public static int IndexOf<T>(this IEnumerable<T> self, Func<T,bool> comparer)
         {
             var array = self.ToArray();
             for (var index = 0; index < array.Length; index++)
             {
                 var testItem = array[index];
                 if (comparer(testItem))
                     return index;
             }

             return -1;
         }

    }
}