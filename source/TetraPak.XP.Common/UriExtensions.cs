using System;
using System.Collections.Generic;
using System.Linq;

namespace TetraPak.XP
{
    // todo Consider moving UriExtensions to a common NuGet package to be referenced instead
    /// <summary>
    ///   Extension operations for <seealso cref="Uri"/> instances.
    /// </summary>
    public static class UriExtensions
    {
        static readonly Dictionary<Uri, Dictionary<string, string>> s_cachedQueries = new();

        /// <summary>
        ///   Looks for a query parameter value in a <see cref="Uri"/>.
        /// </summary>
        /// <param name="uri">
        ///   The <seealso cref="Uri"/> containing the query.
        /// </param>
        /// <param name="key">
        ///   A query property name to look for.
        /// </param>
        /// <returns>
        ///   A <seealso cref="Outcome{T}"/> indicating whether the specified value
        ///   was present in the query. On success this object will also carry the
        ///   requested query property value (<seealso cref="Outcome{T}.Value"/>).
        /// </returns>
        public static Outcome<string> TryGetQueryValue(this Uri uri, string key)
        {
            lock (s_cachedQueries)
            {
                if (!s_cachedQueries.TryGetValue(uri, out var dict))
                {
                    dict = new Dictionary<string, string>();
                    var pairs = uri.Query.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                    for (var i = 0; i < pairs.Length; i++)
                    {
                        var kvp = pairs[i].Split('=');
                        dict[kvp[0].TrimStart('?')] = kvp[1];
                    }

                    s_cachedQueries.Add(uri, dict);
                }

                return dict.TryGetValue(key, out var value)
                    ? Outcome<string>.Success(value)
                    : Outcome<string>.Fail(new KeyNotFoundException($"Query key \"{key}\" was not found"));
            }
        }

        /// <summary>
        ///   Compares a <see cref="Uri"/> with another <seealso cref="Uri"/> to check for
        ///   matching base base paths.
        /// </summary>
        /// <param name="self">
        ///   The <seealso cref="Uri"/>.
        /// </param>
        /// <param name="uri">
        ///   Another <seealso cref="Uri"/> to compare with.
        /// </param>
        /// <param name="enforceMatchingScheme">
        ///   (optional; default=<c>false</c>)<br/>
        ///   Specifies whether the two <see cref="Uri"/> schemes must also match (e.g. "http" / "https").
        /// </param>
        /// <param name="comparison">
        ///   (optional; default = <see cref="StringComparison.OrdinalIgnoreCase"/>)<br/>
        ///   Specifies the string comparison method.
        /// </param>
        /// <returns></returns>
        public static bool EqualsBasePath(this Uri self, Uri uri, bool enforceMatchingScheme = true, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            if (!self.Authority.Equals(uri.Authority, comparison))
                return false;

            if (enforceMatchingScheme && !self.Scheme.Equals(uri.Scheme))
                return false;

            if (self.AbsolutePath.Length != 0 && !uri.AbsolutePath.StartsWith(self.AbsolutePath, comparison))
                return false;

            return true;
        }
    }
}
