using System;
using System.Collections.Generic;
using System.Text;
using TetraPak.XP.StringValues;

namespace TetraPak.XP.CLI
{
    public sealed class CommandLineArgs : MultiStringValue
    {
        protected override StringValueParseResult OnParse(string? stringValue)
        {
            Separator = " ";
            var outcome = tryParse(stringValue);
            if (!outcome)
            {
                Items = Array.Empty<string>();
                return base.OnParse(AsError(outcome.Message));
            }

            Items = outcome.Value!;
            IsEmpty = Items.Length == 0;
            return new StringValueParseResult(stringValue!, stringValue?.GetHashCode() ?? 0);        
        }
        
        static Outcome<string[]> tryParse(string? value)
        {
            if (value.IsUnassigned())
                return Outcome<string[]>.Success(Array.Empty<string>());

            // a simple split won't cut it for command lines as there might be white space inside of string literals ...
            var ca = value!.ToCharArray();
            var sb = new StringBuilder();
            var isStringLiteral = false;
            var items = new List<string>();
            for (var i = 0; i < ca.Length; i++)
            {
                var c = ca[i];
                if (char.IsWhiteSpace(c))
                {
                    if (isStringLiteral)
                    {
                        sb.Append(c);
                        continue;
                    }

                    if (sb.Length == 0)
                        continue;
                    
                    items.Add(sb.ToString());
                    sb.Clear();
                    continue;
                }
                
                if (c is '\"' or '\'')
                {
                    if (!isStringLiteral)
                    {
                        isStringLiteral = true;
                        continue;
                    }
                    
                    isStringLiteral = false;
                    items.Add(sb.ToString());
                    sb.Clear();
                    continue;
                }

                sb.Append(c);
            }

            if (sb.Length != 0)
            {
                items.Add(sb.ToString());
            }

            return Outcome<string[]>.Success(items.ToArray());
        }

        public static implicit operator CommandLineArgs(string? stringValue) => new(stringValue);

        public static implicit operator CommandLineArgs(string[] items) => new(items);

        public CommandLineArgs(string? stringValue)
        : base(stringValue, " ")
        {
        }
        
        public CommandLineArgs(IEnumerable<string>? items)
        : base(items.ConcatEnumerable(" "))
        {
        }

    }
}