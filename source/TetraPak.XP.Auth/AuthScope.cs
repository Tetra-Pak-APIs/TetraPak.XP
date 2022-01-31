using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TetraPak.XP.Auth.OIDC;

namespace TetraPak.XP.Auth
{
    /// <summary>
    ///   Represents supported OAuth/OIDC scopes.
    /// </summary>
    [DebuggerDisplay("{" + nameof(StringValue) + "}")]
    public class AuthScope
    {
        const char Separator = ' ';
        const string CacheKey = "authScopes";

        // todo Add documentation/explanations for all scopes
        public static readonly AuthScope OpenId = "openid";
        public static readonly string Profile = "profile";
        public static readonly string Email = "email";
        public static readonly string General = "general";
        public static readonly string Groups = "groups";

        internal static bool IsDiscovered { get; private set; }
        
        /// <summary>
        ///   Gets the scope string value.
        /// </summary>
        public string StringValue { get; }

        /// <summary>
        ///   Gets a value to indicate whether the scope is empty/unassigned.
        /// </summary>
        public bool IsEmpty => string.IsNullOrEmpty(StringValue);

        /// <summary>
        ///   Returns the number of individual scope identifiers.
        /// </summary>
        public int Count => Items?.Length ?? 0;

        /// <summary>
        ///   Returns an empty <see cref="AuthScope"/> value.
        /// </summary>
        public static AuthScope Empty { get; } = new AuthScope();

        /// <summary>
        ///   Returns all currently supported scopes as items of an array. Please note that
        ///   for this value to be accurate, the <see cref="DiscoveryDocument"/> must be p to date.
        /// </summary>
        /// <seealso cref="DiscoveryDocument"/>
        public static string[] Supported => DiscoveryDocument.Current?.ScopesSupported.ToArray();

        /// <summary>
        ///   Returns all currently known scopes as items of an array. One or more of these
        ///   scopes might not be supported.
        /// </summary>
        /// <seealso cref="Supported"/>
        public static string[] Wellknown { get; } = { OpenId, Profile, Email, General, Groups };

        /// <summary>
        ///   Returns the individual scope identifiers as items in a <see cref="string"/> array.
        /// </summary>
        public string[] Items { get; }

        /// <summary>
        ///   Gets a value indicating whether the scope is fully supported.
        /// </summary>
        /// <seealso cref="IsScopeSupported"/>
        public bool IsSupported => IsScopeSupported(this, out _);

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
        public static bool IsScopeSupported(string scope, out string unsupportedScope)
        {
            return isSupported(scope, out _, out unsupportedScope);
        }

        public override string ToString() => StringValue;

        static bool isSupported(string scope, out string[] items, out string unsupportedScope)
        {
            if (Supported is null)
            {
                items = new string[0];
                unsupportedScope = scope;
                return false;
            }

            var test = scope.Trim();
            if (!test.Contains(Separator))
            {
                unsupportedScope = string.Empty;
                if (test.Length == 0)
                {
                    items = new string[0];
                    return true;
                }

                if (Supported.Any(i => i.Equals(test, StringComparison.InvariantCultureIgnoreCase)))
                {
                    items = new[] { test };
                    return true;
                }

                unsupportedScope = test;
                items = new string[0];
                return false;
            }

            if (!tryParse(ref test, out items))
                throw invalidFormatError(scope);

            var unsupported = new StringBuilder();
            for (var i = 0; i < items.Length; i++)
            {
                if (IsScopeSupported(items[i], out _)) 
                    continue;

                unsupported.Append(items[i]);
                unsupported.Append(Separator);
            }

            if (unsupported.Length != 0)
            {
                unsupportedScope = unsupported.ToString().TrimEnd();
                items = new string[0];
                return false;
            }

            unsupportedScope = string.Empty;
            return true;

        }

        static bool tryParse(ref string stringValue, out string[] scopes)
        {
            var split = stringValue?.Split(new[] {Separator}, StringSplitOptions.RemoveEmptyEntries);
            if (split is null || split.Length == 0)
            {
                scopes = null;
                return false;
            }

            scopes = new string[split.Length];
            for (var i = 0; i < split.Length; i++)
            {
                var scope = split[i].Trim();
                if (!IsScopeSupported(scope, out var unsupportedScope))
                    throw unsupportedScopeError(unsupportedScope);

                scopes[i] = split[i].Trim();
            }

            stringValue = stringValue.Trim();
            return true;
        }

        static Exception invalidFormatError(string stringValue) => new FormatException($"Invalid scope: { stringValue }");

        // ReSharper disable once NotResolvedInText
        static Exception unsupportedScopeError(string unsupportedScope) => new ArgumentOutOfRangeException("scope", $"Unsupported scope: {unsupportedScope}");

        static string trimAndMerge(IEnumerable<string> scopes, out string[] trimmed)
        {
            var sb = new StringBuilder();
            var trimmedList = new List<string>();
            foreach (var scope in scopes)
            {
                var trimmedScope = scope.Trim();
                if (trimmedScope.Length == 0)
                    continue;

                if (trimmedScope.Contains(Separator))
                {
                    var nestedMerged = trimAndMerge(trimmedScope.Split(Separator), out var trimmedNested);
                    trimmedList.AddRange(trimmedNested);
                    sb.Append(nestedMerged);
                    sb.Append(Separator);
                    continue;
                }
                trimmedList.Add(trimmedScope);
                sb.Append(trimmedScope);
                sb.Append(Separator);
            }

            trimmed = trimmedList.ToArray();
            return sb.ToString().TrimEnd();
        }

        public AuthScope(params string[] scopes)
        {
            if (scopes.Length == 0)
            {
                Items = new string[0];
                return;
            }

            var merged = trimAndMerge(scopes, out var trimmed);
            if (merged.Length == 0)
            {
                Items = new string[0];
                return;
            }

            string[] items;
            if (!IsDiscovered)
            {
                items = trimmed;
            }
            else if (!isSupported(merged, out items, out var unsupportedScope))
                throw unsupportedScopeError(unsupportedScope);

            Items = items;
            StringValue = merged;
        }

        /* obsolete
        /// <summary>
        ///   Initializes the <seealso cref="AuthScope"/> from a specified JWT (id) token.
        /// </summary>
        /// <param name="input">
        ///   Either a URL for the well-known discovery endpoint or a (serialized) JWT token to be used for
        ///   resolving the URL.
        /// </param>
        /// <returns>
        ///   A value indicating initialization success and also carries an <seealso cref="AuthScope"/> value.
        /// </returns>
        public static async Task<BoolValue<AuthScope>> Initialize(string input = null)
        {
            if (input is null)
            {
                if (DiscoveryDocument.Current is { })
                    return Initialize(DiscoveryDocument.Current);

                if (!DiscoveryDocument.TryLoadCached())
                    return BoolValue<AuthScope>.Fail("Failed to discover supported auth scope (no input was provided)");

                return DiscoveryDocument.Current is { } 
                    ? Initialize(DiscoveryDocument.Current) 
                    : BoolValue<AuthScope>.Fail("Cannot initialize supported auth scope without information required for automatic discovery");
            }

            var discoveryDocument = await DiscoveryDocument.DownloadAsync(input);
            return discoveryDocument 
                ? Initialize(discoveryDocument.Value) 
                : BoolValue<AuthScope>.Fail($"Failed to discover supported auth scope from provided information: \"{input}\"");
        }

        /// <summary>
        ///   Initializes the <seealso cref="AuthScope"/> from the scoped supported via a
        ///   <seealso cref="DiscoveryDocument"/>.
        /// </summary>
        /// <param name="discoveryDocument">
        ///   (optional)
        ///   A <seealso cref="DiscoveryDocument"/> used to discover the supported scopes.
        /// </param>
        /// <returns>
        ///   A value indicating initialization success and also carries an <seealso cref="AuthScope"/> value.
        /// </returns>
        public static BoolValue<AuthScope> Initialize(DiscoveryDocument discoveryDocument = null)
        {
            try
            {
                discoveryDocument ??= DiscoveryDocument.Current;
                var authScope = new AuthScope(discoveryDocument.ScopesSupported.ToArray());
                return BoolValue<AuthScope>.Success(authScope);
            }
            catch (Exception ex)
            {
                return BoolValue<AuthScope>.Fail($"Failed while initializing supported auth scopes. {ex.Message}", ex);
            }
        }
        */

        #region .  Implicit Conversion  .
        public static implicit operator string(AuthScope authScope) => authScope?.StringValue;

        public static implicit operator AuthScope(string stringValue) => new AuthScope(stringValue);
        #endregion

        #region .  Equality  .

        public static bool operator ==(AuthScope left, AuthScope right)
        {
            return left?.isEqual(right) ?? right is null;
        }

        public static bool operator !=(AuthScope left, AuthScope right)
        {
            return !(left == right);
        }

        private bool isEqual(AuthScope other)
        {
            if (IsEmpty)
                return other.IsEmpty;

            if (Count != other.Count)
                return false;

            return Items.All(scope => other.Items.Any(i => i.Equals(scope, StringComparison.InvariantCultureIgnoreCase)));
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return isEqual((AuthScope)obj);
        }

        public override int GetHashCode()
        {
            return (Items != null ? Items.GetHashCode() : 0);
        }

        #endregion

        /* obsolete

        static async void tryInitializeFromCacheAsync()
        {
            var cache = DependencyService.Get<ICache<string>>();
            if (cache is null)
                return;

            var cached = await cache.TryGetAsync(CacheKey);
            if (!cached)
                return;

            var authScope = (AuthScope) cached.Value;
            Supported = authScope.Items;
        }
    
        internal static async void Discover(DiscoveryDocument discoveryDocument)
        {
            if (IsDiscovered || discoveryDocument is null)
                return;
            
            Supported = discoveryDocument.ScopesSupported.ToArray();
            IsDiscovered = true;
            
            if (!cacheWhenSuccessful)
                return;

            var supported = merge(Supported, out _);
            var cache = DependencyService.Get<ICache<string>>();
            if (cache is null)
                return;

            await cache.AddAsync(CacheKey, supported);
        }

        public AuthScope() : this(true)
        {
        }

        public AuthScope(bool initFromCacheWhenNotDiscovered)
        {
            if (!IsDiscovered && initFromCacheWhenNotDiscovered)
                tryInitializeFromCacheAsync();
        }
        */
        
        public AuthScope()
        {
        }
    }

    public static class AuthScopeExtensions
    {
        public static string UrlEncoded(this AuthScope self)
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