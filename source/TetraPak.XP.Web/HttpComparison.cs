using System;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace TetraPak.AspNet
{
    /// <summary>
    ///   A string compatible (criteria) expression for use with HTTP requests.
    /// </summary>
    public class HttpComparison : StringValueBase
    {
        static readonly Regex s_regex = new Regex(@"(?<element>[a-zA-Z]+)\s*\[\s*(?<key>.+)\s*\]\s*(?<operator>[\=\!]+)\s*(?<value>.+)", 
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
        
        /// <summary>
        ///   Specifies recognized elements of a HTTP request, for use in comparison operations.
        /// </summary>
        public static class Elements
        {
            /// <summary>
            ///   Represents request headers collection.
            /// </summary>
            public const string Headers = "headers";
            
            /// <summary>
            ///   Represents request query.
            /// </summary>
            public const string Query = "query";
        }

        /// <summary>
        ///   Specifies recognized comparison operators.
        /// </summary>
        public static class Operators
        {
            /// <summary>
            ///   The "is equal" operator.
            /// </summary>
            public const string IsEqual = "==";

            /// <summary>
            ///   The "is not equal" operator.
            /// </summary>
            public const string IsNotEqual = "!=";
        }

        /// <summary>
        ///   Gets the element (<see cref="Elements.Headers"/> or <see cref="Elements.Query"/>)
        ///   references in the operation. 
        /// </summary>
        public HttpRequestElement Element { get; private set; }

        /// <summary>
        ///   Identifies an item from the specified <see cref="Element"/>,
        ///   to be used for comparison with <see cref="Value"/>. 
        /// </summary>
        public string? Key { get; private set; }

        /// <summary>
        ///   Gets the item's value (identified by <see cref="Key"/>) from a request.
        /// </summary>
        /// <param name="request">
        ///   The <see cref="HttpRequest"/>
        /// </param>
        /// <returns>
        ///   A <see cref="string"/> value if the item can be found in the specified <see cref="Element"/>;
        ///   otherwise <c>null</c>.
        /// </returns>
        public string? ItemValue(HttpRequest request) => request.GetItemValue(Element, Key!);

        /// <summary>
        ///   Specifies the comparative operator.
        /// </summary>
        /// <seealso cref="ComparisonOperation"/>
        public ComparisonOperation Operation { get; private set; }

        /// <summary>
        ///   Gets the value to be matched with the specified <see cref="Key"/>. 
        /// </summary>
        public string? Value { get; private set; }

        /// <summary>
        ///   Executes the  specified operation and returns a value indicating a match. 
        /// </summary>
        /// <param name="request">
        ///   The <see cref="HttpRequest"/> to be matched by the operation.
        /// </param>
        /// <param name="comparison">
        ///   (optional; default=<see cref="StringComparison.InvariantCulture"/>)<br/>
        ///   Specifies how to match the <see cref="Value"/> (<see cref="Element"/> and <see cref="Key"/>
        ///   are always case-insensitive.
        /// </param>
        /// <returns>
        ///   <c>true</c> if <see cref="ItemValue"/> matches <see cref="Value"/>;
        ///   otherwise <c>false</c>.
        /// </returns>
        public bool IsMatch(HttpRequest request, StringComparison comparison = StringComparison.InvariantCulture)
        {
            var itemValue = ItemValue(request);
            return itemValue is not null && isMatch(itemValue, comparison);
        }

        /// <summary>
        ///   Overrides base method to resolve <see cref="Element"/>, <see cref="Key"/> and <see cref="Value"/>.
        /// </summary>
        /// <param name="stringValue"></param>
        /// <returns>
        ///   The <paramref name="stringValue"/> if parsing is successful; otherwise <c>null</c>.
        /// </returns>
        protected override string? OnParse(string? stringValue)
        {
            var input = stringValue?.Trim();
            if (string.IsNullOrEmpty(input))
                return null;

            var match = s_regex.Match(input);
            if (!match.Success)
                return null;

            var element = match.Groups["element"].Value.ToLowerInvariant() switch
            {
                Elements.Headers => HttpRequestElement.Header,
                Elements.Query => HttpRequestElement.Query,
                _ => HttpRequestElement.None
            };

            if (element == HttpRequestElement.None)
                return null;

            var operation = match.Groups["operator"].Value switch
            {
                Operators.IsEqual => ComparisonOperation.IsEqual,
                Operators.IsNotEqual => ComparisonOperation.IsNotEqual,
                _ => ComparisonOperation.None
            };
            if (operation is ComparisonOperation.None)
                return null;
                
            Element = element;
            Operation = operation;
            Key = match.Groups["key"].Value!;
            Value = match.Groups["value"].Value!;
            return stringValue;
        }

        bool isMatch(string value, StringComparison comparison)
        {
            return Operation switch
            {
                ComparisonOperation.IsEqual => value.Equals(Value, comparison),
                ComparisonOperation.IsNotEqual => !value.Equals(Value, comparison),
                _ => false
            };
        }

        /// <summary>
        ///   Implicit type cast operation <see cref="string"/> => <see cref="HttpComparison"/>. 
        /// </summary>
        /// <param name="stringValue">
        ///   The textual representation of a <see cref="HttpComparison"/> value.
        /// </param>
        /// <returns></returns>
        public static implicit operator HttpComparison(string? stringValue) => new(stringValue);

        /// <summary>
        ///   Initializes the <see cref="HttpComparison"/>. 
        /// </summary>
        /// <param name="value">
        ///   A textual representation of a <see cref="HttpComparison"/> value.
        /// </param>
        public HttpComparison(string? value)
        : base(value)
        {
            
        }
    }
    
    /// <summary>
    ///   Used to express a HTTP request element.
    /// </summary>
    /// <seealso cref="HttpComparison"/>
    public enum HttpRequestElement
    {
        /// <summary>
        ///   No element is specified.
        /// </summary>
        None,
        
        /// <summary>
        ///   Specifies the <see cref="HttpRequest.Query"/> element.
        /// </summary>
        Query,
        
        /// <summary>
        ///   Specifies the <see cref="HttpRequest.Headers"/> element.
        /// </summary>
        Header,
        
        /// <summary>
        ///   Specifies the <see cref="HttpRequest.Body"/> element.
        /// </summary>
        Body
    }

    /// <summary>
    ///   used to express a comparison operation.
    /// </summary>
    public enum ComparisonOperation
    {
        /// <summary>
        ///   No (recognized) comparison operation is expressed. 
        /// </summary>
        None,
        
        /// <summary>
        ///   Specified the "is equal" operation.
        /// </summary>
        IsEqual,
        
        /// <summary>
        ///   Specified the "is not equal" operation.
        /// </summary>
        IsNotEqual
    }
}