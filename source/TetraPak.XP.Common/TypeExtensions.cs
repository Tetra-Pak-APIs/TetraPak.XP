using System;

namespace TetraPak.XP
{
    // todo Consider moving TypeExtensions to a common NuGet package to be referenced instead
    /// <summary>
    ///   Helpers for type/reflection related operations.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        ///   Analyzes a <seealso cref="Type"/> and returns a value indicating whether
        ///   it is derived from another <seealso cref="Type"/>.
        /// </summary>
        /// <param name="derivedType">
        ///   The (potentially derived) type to be tested.
        /// </param>
        /// <param name="baseType">
        ///   A type to be tested as a base type.
        /// </param>
        /// <returns>
        ///   <c>true</c> if <paramref name="derivedType"/> derives from <paramref name="baseType"/>;
        ///   otherwise <c>false</c>.
        /// </returns>
        public static bool IsDerivedFrom(this Type derivedType, Type baseType)
        {
            while (derivedType != null && derivedType != typeof(object))
            {
                var cur = derivedType.IsGenericType ? derivedType.GetGenericTypeDefinition() : derivedType;
                if (baseType == cur)
                    return true;

                derivedType = derivedType.BaseType;
            }
            return false;
        }

        // /// <summary>
        // ///   Examines a specified value and returns a value to indicate whether
        // ///   it is a nullable value.
        // /// </summary>
        // /// <param name="_">
        // ///   The value to be examined.
        // /// </param>
        // /// <typeparam name="T">
        // ///   The type of value.
        // /// </typeparam>
        // /// <returns>
        // ///   <c>true</c> if the value is nullable; otherwise <c>false</c>.
        // /// </returns>
        // public static bool IsNullable<T>(this T _) => Nullable.GetUnderlyingType(typeof(T)) != null;
        //
        // /// <summary>
        // ///   Returns a value indicating whether the type implements a specified interface.
        // /// </summary>
        // /// <param name="type">
        // ///   The type to be examined.
        // /// </param>
        // /// <param name="interface">
        // ///   The requested interface. 
        // /// </param>
        // /// <returns>
        // ///   <c>true</c> if <paramref name="type"/> implements the interface; otherwise <c>false</c>.
        // /// </returns>
        // /// <exception cref="ArgumentNullException">
        // ///   <paramref name="type"/> or <paramref name="interface"/> was unassigned (<c>null</c>).
        // /// </exception>
        // /// <exception cref="ArgumentException">
        // ///   <paramref name="interface"/> was not an interface.
        // /// </exception>
        // public static bool IsImplementingInterface(this Type type, Type @interface)
        // {
        //     if (type is null)
        //         throw new ArgumentNullException(nameof(type));
        //
        //     if (@interface is null)
        //         throw new ArgumentNullException(nameof(@interface));
        //
        //     if (!@interface.IsInterface)
        //         throw new ArgumentException($"Not an interface: {@interface}");
        //
        //     var interfaces = type.GetInterfaces();
        //     if (@interface.IsGenericTypeDefinition)
        //     {
        //         foreach (var item in interfaces)
        //         {
        //             if (item.IsConstructedGenericType && item.GetGenericTypeDefinition() == @interface)
        //                 return true;
        //         }
        //
        //         return false;
        //     }
        //
        //     foreach (var item in interfaces)
        //     {
        //         if (item == @interface)
        //             return true;
        //     }
        //
        //     return false;
        // }
    }
}
