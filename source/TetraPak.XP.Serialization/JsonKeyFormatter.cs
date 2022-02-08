using System;
using System.Text;

namespace TetraPak.XP.Serialization
{
    public static class JsonKeyFormatter
    {
        public static string ToJsonKeyFormat(this string s, KeyTransformationFormat transformationFormat)
        {
            return transformationFormat switch
            {
                KeyTransformationFormat.CamelCase => s.ToCamelCaseIdentifier(),
                KeyTransformationFormat.PascalCase => s.ToPascalCaseIdentifier(),
                KeyTransformationFormat.SnakeCase => s.ToSnakeCaseIdentifier(),
                KeyTransformationFormat.KebabCase => s.ToKebabCaseIdentifier(),
                KeyTransformationFormat.None => s,
                _ => throw new ArgumentOutOfRangeException(nameof(transformationFormat))
            };
        }
        
        /// <summary>
        ///   Returns the camelCase version of a <see cref="string"/>. 
        /// </summary>
        public static string ToCamelCaseIdentifier(this string s)
        {
            return toAlphanumericIdentifier(s, InitialCasing.Lower);
        }

        /// <summary>
        ///   Returns the PascalCase version of a <see cref="string"/>. 
        /// </summary>
        public static string ToPascalCaseIdentifier(this string s)
        {
            return toAlphanumericIdentifier(s, InitialCasing.Upper);
        }

        /// <summary>
        ///   Returns the snake_case version of a <see cref="string"/>. 
        /// </summary>
        public static string ToSnakeCaseIdentifier(this string s) => toLowerCaseIdentifierWithSeparator(s, '_');

        /// <summary>
        ///   Returns the kebab-case version of a <see cref="string"/>. 
        /// </summary>
        public static string ToKebabCaseIdentifier(this string s) => toLowerCaseIdentifierWithSeparator(s, '-');
        
        static string toAlphanumericIdentifier(string s, InitialCasing initialCasing)
        {
            var ca = s.ToCharArray();
            var sb = new StringBuilder();
            var toUpper = false;
            for (var i = 0; i < ca.Length; i++)
            {
                var c = ca[i];
                if (char.IsLetter(c))
                {
                    if (i == 0)
                    {
                        c = initialCasing switch
                        {
                            InitialCasing.Lower => char.ToLower(c),
                            InitialCasing.Upper => char.ToUpper(c),
                            _ => throw new NotSupportedException($"Unsupported {typeof(InitialCasing)} value: {initialCasing.ToString()}")
                        };
                    }

                    if (toUpper)
                    {
                        c = char.ToUpper(c);
                        toUpper = false;
                    }
                    
                    sb.Append(c);
                    continue;
                }

                if (char.IsDigit(c))
                {
                    sb.Append(c);
                    continue;
                }

                toUpper = true;
            }

            return sb.ToString();
        }

        static string toLowerCaseIdentifierWithSeparator(string s, char separator)
        {
            var ca = s.ToCharArray();
            var sb = new StringBuilder();
            var isSeparatorInserted = false;
            for (var i = 0; i < ca.Length; i++)
            {
                var c = ca[i];
                if (c == separator || char.IsDigit(c))
                {
                    sb.Append(c);
                    isSeparatorInserted = c == separator;
                    continue;
                }

                if (!char.IsLetter(c))
                {
                    if (!isSeparatorInserted)
                    {
                        sb.Append(separator);
                    }
                    isSeparatorInserted = true;
                    continue;
                }
                
                if (char.IsUpper(c) && i != 0 && !isSeparatorInserted)
                {
                    sb.Append(separator);
                    isSeparatorInserted = true;
                }
                c = char.ToLower(c);
                sb.Append(c);
            }

            return sb.ToString();
        }

        enum InitialCasing { Upper, Lower }
    }
    
    /// <summary>
    ///   Used to specify a format for transforming keys (such as in JSON serialization).
    /// </summary>
    public enum KeyTransformationFormat
    {
        None,

        CamelCase,
        
        PascalCase,
        
        SnakeCase,
        
        KebabCase,
    }
}