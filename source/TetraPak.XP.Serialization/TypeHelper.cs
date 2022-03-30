using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using TetraPak.XP.Logging.Abstractions;
using TetraPak.XP.StringValues;
#if NET5_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif

namespace TetraPak.XP.Serialization
{
    public static class TypeHelper
    {
        public static event DeserializationHandler? Deserialize;

        public static bool TryDeserializeStringValue(
            this Type targetType,
            string s, 
#if NET5_0_OR_GREATER
            [NotNullWhen(true)] out object? result,
#else
            out object? result,
#endif
            ILog? log = null)
        {
            if (tryIocDeserialize(s, out result, log))
                return true;

            if (tryDeserializeNumeric(s, targetType, out result))
                return true;

            if (!targetType.IsImplementingInterface<IStringValue>())
                return false;

            const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var ctor = targetType.GetConstructor(Flags, null, new[] { typeof(string) }, null);
            if (ctor == null)
            {
                // try casting (IStringValue implementations should support implicit type casting from string) ...
                var implicitOp = targetType.GetMethod("op_Implicit", new[] { typeof(string) });
                if (implicitOp == null)
                    throw new Exception($"Cannot deserialize '{s}' of type {targetType}. Type does not support ctor with single string parameter.");
                
                result = implicitOp.Invoke(null, new object[] {s});
                return result is {};
            }

            result = ctor.Invoke(new object[] { s });
            return true;
        }
        
        #if NET5_0_OR_GREATER        
        static bool tryIocDeserialize(string serialized, [NotNullWhen(true)] out object? deserialized, ILog? log)
#else
        static bool tryIocDeserialize(string serialized, out object? deserialized, ILog? log)
#endif
        {
            deserialized = default;
            if (Deserialize == null)
                return false;

            var handlers = Deserialize.GetInvocationList().Cast<DeserializationHandler>();
            foreach (var handler in handlers)
            {
                try
                {
                    if (!handler(serialized, out var value, true))
                        continue;

                    deserialized = value;
                    return true;
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    throw;
                }
            }

            return false;
        }

#if NET5_0_OR_GREATER        
        static bool tryDeserializeNumeric(string s, Type targetType, [NotNullWhen(true)] out object? result)
#else        
        static bool tryDeserializeNumeric(string s, Type targetType, out object? result)
#endif
        {
            switch (Type.GetTypeCode(targetType))
            {
                case TypeCode.Byte:
                    if (!byte.TryParse(s, NumberStyles.Number, Parsing.FormatProvider, out var byteResult))
                        break;

                    result = byteResult;
                    return true;

                case TypeCode.SByte:
                    if (!sbyte.TryParse(s, NumberStyles.Number, Parsing.FormatProvider, out var sbyteResult))
                        break;

                    result = sbyteResult;
                    return true;

                case TypeCode.Int16:
                    if (!short.TryParse(s, NumberStyles.Number, Parsing.FormatProvider, out var shortResult))
                        break;

                    result = shortResult;
                    return true;

                case TypeCode.Int32:
                    if (!int.TryParse(s, NumberStyles.Number, Parsing.FormatProvider, out var int32Result))
                        break;

                    result = int32Result;
                    return true;

                case TypeCode.Int64:
                    if (!long.TryParse(s, NumberStyles.Number, Parsing.FormatProvider, out var int64Result))
                        break;

                    result = int64Result;
                    return true;

                case TypeCode.UInt16:
                    if (!ushort.TryParse(s, NumberStyles.Number, Parsing.FormatProvider, out var uint16Result))
                        break;

                    result = uint16Result;
                    return true;

                case TypeCode.UInt32:
                    if (!uint.TryParse(s, NumberStyles.Number, Parsing.FormatProvider, out var uint32Result))
                        break;

                    result = uint32Result;
                    return true;

                case TypeCode.UInt64:
                    if (!ulong.TryParse(s, NumberStyles.Number, Parsing.FormatProvider, out var uint64Result))
                        break;

                    result = uint64Result;
                    return true;

                case TypeCode.Decimal:
                    if (!decimal.TryParse(s, NumberStyles.Number, Parsing.FormatProvider, out var decimalResult))
                        break;

                    result = decimalResult;
                    return true;

                case TypeCode.Single:
                    if (!float.TryParse(s, NumberStyles.Number, Parsing.FormatProvider, out var singleResult))
                        break;

                    result = singleResult;
                    return true;

                case TypeCode.Double:
                    if (!double.TryParse(s, NumberStyles.Number, Parsing.FormatProvider, out var doubleResult))
                        break;

                    result = doubleResult;
                    return true;
            }
            result = null;
            return false;
        }
    }
}