using System;
using System.Collections.Generic;
using System.Linq;
using TetraPak.XP.StringValues;

namespace TetraPak.XP.Configuration
{
    public static class Configure
    {
        static readonly List<IConfigurationDecoratorDelegate> s_decorators = new();
        static readonly List<IConfigurationValueDelegate> s_valueDelegates = new();
        static readonly List<ValueParser> s_valueParsers = getDefaultValueParsers();

        internal static IConfigurationDecoratorDelegate[] GetConfigurationDecorators()
        {
            lock (s_decorators)
                return s_decorators.ToArray();
        }

        internal static IConfigurationValueDelegate[] GetValueDelegates()
        {
            lock (s_valueDelegates)
                return s_valueDelegates.ToArray();
        }

        internal static ValueParser[] GetValueParsers()
        {
            lock (s_valueParsers)
                return s_valueParsers.ToArray();
        }

        public static void InsertConfigurationDecorator(
            IConfigurationDecoratorDelegate decoratorDelegate,
            int index = -1)
        {
            lock (s_decorators)
            {
                if (s_decorators.Contains(decoratorDelegate))
                    throw new ArgumentException("Decorator was already inserted", nameof(decoratorDelegate));

                if (index >= 0)
                {
                    s_decorators.Insert(index, decoratorDelegate);
                    return;
                }

                if (!decoratorDelegate.IsFallbackDecorator || s_decorators.Count == 0)
                {
                    s_decorators.Insert(0, decoratorDelegate);
                    return;
                }

                // insert fallback decorator ...
                var lastIndex = s_decorators.Count - 1;
                for (var i = s_decorators.Count - 1; i >= 0; i--)
                {
                    var decorator = s_decorators[i];
                    if (decorator.IsFallbackDecorator)
                    {
                        s_decorators.Insert(lastIndex, decoratorDelegate);
                        return;
                    }

                    lastIndex = i;
                }

                s_decorators.Insert(0, decoratorDelegate);
            }
        }

        public static void InsertValueDelegateBefore<T>(IConfigurationValueDelegate valueDelegate)
        where T : IConfigurationValueDelegate
        {
            int index;
            lock (s_valueDelegates)
            {
                index = s_valueDelegates.IndexOf(vDelegate => vDelegate is T);
            }
            InsertValueDelegate(valueDelegate, index);
        }

        /// <summary>
        ///   Inserts a custom configuration value delegate.
        /// </summary>
        /// <param name="valueDelegate">
        ///   The value delegate to be inserted.
        /// </param>
        /// <param name="index">
        ///   (optional; default=-1)<br/>
        ///   Specifies at what position to insert the delegate. Passing a negative value equates to zero (0)
        ///   and will insert the delegate as the first one in the internal list of delegates (the first one
        ///   to be invoked when values are being requested from the configuration framework).
        ///   Passing a positive value will insert the delegate at the specified position.
        ///   Please note that if the delegate is marked as a "fallback delegate", index will be ignored
        ///   and the delegate will always be invoked only when other (non-fallback 
        /// </param>
        /// <exception cref="ArgumentException">
        ///   The value delegate was already inserted.
        /// </exception>
        public static void InsertValueDelegate(IConfigurationValueDelegate valueDelegate, int index = -1)
        {
            lock (s_valueDelegates)
            {
                if (s_valueDelegates.Contains(valueDelegate))
                    throw new ArgumentException("Delegate was already inserted", nameof(valueDelegate));

                if (valueDelegate.IsFallbackDelegate)
                {
                    insertFallbackDelegate();
                    return;
                }
                    
                if ( index >= 0)
                {
                    s_valueDelegates.Insert(index, valueDelegate);
                    return;
                }

                s_valueDelegates.Insert(0, valueDelegate);

                void insertFallbackDelegate()
                {
                    var idxFallbackDelegate = s_valueDelegates.IndexOf(d => d.IsFallbackDelegate);
                    if (idxFallbackDelegate == -1)
                    {
                        s_valueDelegates.Add(valueDelegate);
                        return;
                    }

                    if (index < 0)
                    {
                        s_valueDelegates.Insert(idxFallbackDelegate, valueDelegate);
                        return;
                    }

                    index = Math.Max(s_valueDelegates.Count-1, idxFallbackDelegate + index);
                    s_valueDelegates.Insert(index, valueDelegate);
                }
            }
        }

        public static void InsertValueParser(ValueParser parser, int index = -1)
        {
            lock (s_valueParsers)
            {
                s_valueParsers.Insert(Math.Max(0, index), parser);
            }
        }

        static List<ValueParser> getDefaultValueParsers()
        {
            // automatically support ...
            return new ValueParser[]
            {
                // string
                (string? stringValue, Type tgtType, out object? o, object useDefault) =>
                {
                    if (tgtType != typeof(string))
                    {
                        o = null!;
                        return false;
                    }

                    var s = stringValue?.Trim();
                    o = string.IsNullOrEmpty(s) ? useDefault : s;
                    return true;
                },

                // IStringValue
                (string? stringValue, Type tgtType, out object? o, object useDefault) =>
                {
                    if (!typeof(IStringValue).IsAssignableFrom(tgtType))
                    {
                        o = null!;
                        return false;
                    }

                    var s = stringValue?.Trim();
                    o = string.IsNullOrEmpty(s) ? useDefault : StringValueBase.MakeStringValue(tgtType, s);
                    return true;
                },

                // boolean
                (string? stringValue, Type tgtType, out object? o, object useDefault) =>
                {
                    if (tgtType != typeof(bool))
                    {
                        o = null!;
                        return false;
                    }

                    var s = stringValue?.Trim();
                    if (string.IsNullOrEmpty(s))
                    {
                        o = useDefault;
                        return true;
                    }

#if NET5_0_OR_GREATER
                if (s.TryParseConfiguredBool(out var boolValue))
#else
                    if (s!.TryParseConfiguredBool(out var boolValue))
#endif
                    {
                        o = boolValue;
                        return true;
                    }

                    o = null!;
                    return false;
                },

                // numeric
                (string? stringValue, Type tgtType, out object? o, object useDefault) =>
                {
                    if (tgtType.IsNumeric() && stringValue.TryParseNumeric(tgtType, out o))
                        return true;

                    o = useDefault;
                    return false;
                },

                // enum
                (string? stringValue, Type tgtType, out object? o, object useDefault) =>
                {
                    if (!tgtType.IsEnum)
                    {
                        o = null!;
                        return false;
                    }

                    var s = stringValue
                        ?.Trim(); // todo Consider supporting names with whitespace (make identifier). Eg: "Client Credentials" => "ClientCredentials"
                    if (string.IsNullOrEmpty(s))
                    {
                        o = useDefault;
                        return true;
                    }

#if NET5_0_OR_GREATER
                if (s.TryParseEnum(tgtType, out o))
#else
                    if (s!.TryParseEnum(tgtType, out o))
#endif
                        return true;

                    o = null;
                    return false;
                },

                // timespan
                (string? stringValue, Type tgtType, out object? o, object useDefault) =>
                {
                    if (tgtType != typeof(TimeSpan))
                    {
                        o = null!;
                        return false;
                    }

                    var s = stringValue?.Trim();
                    if (string.IsNullOrEmpty(s))
                    {
                        o = useDefault;
                        return true;
                    }

#if NET5_0_OR_GREATER
                if (s.TryParseConfiguredTimeSpan(out var timeSpanValue))
#else
                    if (s!.TryParseConfiguredTimeSpan(out var timeSpanValue))
#endif
                    {
                        o = timeSpanValue;
                        return true;
                    }

                    o = null!;
                    return false;
                }
            }.ToList();
        }
    }
}