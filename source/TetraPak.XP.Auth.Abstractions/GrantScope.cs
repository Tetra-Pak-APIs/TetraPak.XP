using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TetraPak.XP.Auth.Abstractions.OIDC;
using TetraPak.XP.StringValues;

namespace TetraPak.XP.Auth.Abstractions
{
    /// <summary>
    ///   Represents supported OAuth/OIDC scopes.
    /// </summary>
    [DebuggerDisplay("{ToString()}")]
    public sealed class GrantScope : MultiStringValue
    {
        // todo Add documentation/explanations for all scopes
        public const string OpenId = "openid";
        public const string Profile = "profile";
        public const string Email = "email";
        public const string General = "general";
        public const string Groups = "groups";

        public new static GrantScope Empty => new(); 
        
        /// <summary>
        ///   Returns all currently supported scopes as items of an array.
        ///   Please note that for this value to be accurate, the <see cref="DiscoveryDocument"/> must be up to date.
        /// </summary>
        /// <seealso cref="DiscoveryDocument"/>
        public static string[] Supported => DiscoveryDocument.Current?.ScopesSupported is {} 
            ? DiscoveryDocument.Current.ScopesSupported.ToArray() 
            : Array.Empty<string>();

        /// <summary>
        ///   Returns all currently known scopes as items of an array. One or more of these
        ///   scopes might not be supported.
        /// </summary>
        /// <seealso cref="Supported"/>
        public static string[] Wellknown { get; } = { OpenId, Profile, Email, General, Groups };

        /// <summary>
        ///   Gets a value indicating whether the scope is fully supported.
        /// </summary>
        /// <seealso cref="IsScopeSupported"/>
        public Outcome IsSupported => IsScopeSupported(this, out _);

        /// <summary>
        ///   Examines a specified scope and returns a value to indicate whether it is supported.
        /// </summary>
        /// <param name="scope">
        ///   A string value representing one or more scopes (separated by whitespace).
        /// </param>
        /// <param name="unsupportedScope">
        ///   Passes back any unsupported scope identifiers found (separated by whitespace).
        /// </param>
        /// <returns>
        ///   <c>true</c> if all specified scopes are supported; otherwise <c>false</c>.
        /// </returns>
        public static Outcome IsScopeSupported(GrantScope scope, out string[] unsupportedScope) 
            => 
            isScopeSupported(scope.Items, out unsupportedScope);

        static Outcome isScopeSupported(string[] scope, out string[] unsupportedScope)
        {
            if (DiscoveryDocument.Current is null)
            {
                unsupportedScope = Array.Empty<string>();
                return Outcome.Cancel("Please ensure a discovery document is available before checking supported scope");
            }

            var supported = DiscoveryDocument.Current.ScopesSupported?.ToArray() ?? Array.Empty<string>();
            if (!supported.Any())
            {
                unsupportedScope = scope;
                return Outcome.Fail($"Unsupported scoped: {scope}");
            }

            var unsupportedList = new List<string>();
            foreach (var item in scope)
            {
                if (!supported.Any(i => i.Equals(item, StringComparison.InvariantCultureIgnoreCase)))
                    unsupportedList.Add(item);
            }

            unsupportedScope = unsupportedList.ToArray();
            return !unsupportedScope.Any()
                ? Outcome.Success() 
                : Outcome.Fail($"Unsupported scoped: {scope}");
        }
        
        public static bool TryParse(string? stringValue, out GrantScope? value)
        {
            if (!MultiStringValue.TryParseAs(typeof(GrantScope), stringValue, out var msv) 
                || msv is not GrantScope grantScope)
            {
                value = null;
                return false;
            }

            value = grantScope;
            return true;
        }

        /// <summary>
        ///   Overrides base method to automatically validate whether scopes are currently supported,
        ///   as specified by <see cref="DiscoveryDocument.Current"/>. If no <see cref="DiscoveryDocument"/>
        ///   is available (not configured/uninitialised) no validation takes place and a successful outcome
        ///   (false positive) is returned.
        /// </summary>
        /// <param name="items">
        ///   The scope items to be validated.
        /// </param>
        /// <returns>
        ///   A <see cref="Outcome{T}"/> value indicating success/failure and, on success, carrying
        ///   the validated cope items as <see cref="Outcome{T}.Value"/>; otherwise <see cref="Outcome.Exception"/>
        ///   is assigned.
        /// </returns>
        protected override Outcome<string[]> OnValidate(string[] items)
        {
            return DiscoveryDocument.Current is { }
                ? isScopeSupported(Items, out var unsupportedItems)
                    ? Outcome<string[]>.Success(items)
                    : Outcome<string[]>.Fail($"Unsupported scope items: {unsupportedItems.ConcatEnumerable(" ")}")
                : Outcome<string[]>.Success(items);
        }

        public override string ToString() => IsEmpty ? string.Empty : StringValue;

        public static implicit operator GrantScope(string stringValue) => new(stringValue);

        /// <summary>
        ///   Initializes the grant scope with one or more scopes.
        /// </summary>
        /// <param name="scopes">
        ///   One or more scopes to be included.   
        /// </param>
        public GrantScope(params string[] scopes) 
        : base(scopes)
        {
        }
        
        /// <summary>
        ///   Initializes the grant scope with its string representation.
        ///   Individual scopes should be space-separated.
        /// </summary>
        public GrantScope(string stringValue) 
        : base(stringValue)
        {
        }
    }

    public static class GrantScopeExtensions
    {
        public static string UrlEncoded(this GrantScope self)
        {
            var sb = new StringBuilder();
            foreach (var item in self.Items)
            {
                sb.Append(item);
                sb.Append('+');
            }

            return sb.ToString().TrimEnd('+');
        }
        
    }
}