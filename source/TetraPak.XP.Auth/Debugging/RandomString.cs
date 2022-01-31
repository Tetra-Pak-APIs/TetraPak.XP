using System;
using System.Diagnostics;

namespace TetraPak.XP.Auth.Debugging
{
    [DebuggerDisplay("{" + nameof(StringValue) + "}")]
    public class RandomString
    {
        static readonly char[] s_extendedChars = { '-', '%', '_', '#', '@', '^', '~', '!' };
        static readonly char[] s_validChars;
        static readonly char[] s_validExtendedChars;
        static uint s_defaultLength = 32;
        static Random s_random;

        public static uint DefaultLength
        {
            get => s_defaultLength;
            set => s_defaultLength = Math.Max(value, 1u);
        }

        public static Random Random
        {
            get => s_random;
            set => s_random = value ?? new Random(DateTime.Now.Millisecond);
        }

        public string StringValue { get; }

        /// <summary>
        ///   Initializes the value.
        /// </summary>
        /// <param name="length">
        ///   (optional; default = <see cref="DefaultLength"/>)<br/>
        ///   Specifies a length for the random string.
        /// </param>
        /// <param name="allowSpecialCharacters">
        ///   (optional; default = <c>true</c>)<br/>
        ///   Specifies whether special characters (other than alfa numeric ones) are allowed in the random string.    
        /// </param>
        [DebuggerStepThrough]
        public RandomString(uint? length = null, bool allowSpecialCharacters = true)
        {
            length ??= DefaultLength;
            var ca = new char[length.Value];
            var validChars = allowSpecialCharacters ? s_validExtendedChars : s_validChars;
            var max = validChars.Length;
            for (var i = 0; i < ca.Length; i++)
            {
                var index = Random.Next(max);
                ca[i] = validChars[index];
            }
            StringValue = new string(ca);
        }

        /// <summary>
        ///   Implicitly converts a <see cref="RandomString"/> value into its <see cref="string"/> representation.
        /// </summary>
        /// <param name="value">
        ///   A <see cref="RandomString"/> value to be implicitly converted into its <see cref="string"/> representation.
        /// </param>
        /// <returns>
        ///   The <see cref="string"/> representation of <paramref name="value"/>.
        /// </returns>
        public static implicit operator string(RandomString value) => value?.StringValue;

        /// <inheritdoc />
        public override string ToString() => StringValue;
        
        static void makeValidRandomChars(out char[] validChars, out char[] validExtendedChars)
        {
            var i = 0;
            const int Length = ('z' - 'a') + ('Z' - 'A') + ('9' - '0') + 3;
            validChars = new char[Length];
            validExtendedChars = new char[Length + s_extendedChars.Length];
            for (var c = 'a'; c <= 'z'; c++, i++)
            {
                validChars[i] = validExtendedChars[i] = c;
            }
            for (var c = 'A'; c <= 'Z'; c++, i++)
            {
                validChars[i] = validExtendedChars[i] = c;
            }
            for (var c = '0'; c <= '9'; c++, i++)
            {

                validChars[i] = c;
                validExtendedChars[i] = c;
            }
            for (var c = 0; c < s_extendedChars.Length; c++, i++)
            {
                validExtendedChars[i] = s_extendedChars[c];
            }
        }

        static RandomString()
        {
            Random = new Random(DateTime.Now.Millisecond);
            makeValidRandomChars(out var validChars, out var validExtendedChars);
            s_validChars = validChars;
            s_validExtendedChars = validExtendedChars;
        }

        #region .  Equality  .

        /// <summary>
        ///   Determines whether the specified value is equal to the current value.
        /// </summary>
        /// <param name="other">
        ///   A <see cref="RandomString"/> value to compare to this value.
        /// </param>
        /// <returns>
        ///   <c>true</c> if <paramref name="other"/> is equal to the current value; otherwise <c>false</c>.
        /// </returns>
        public bool Equals(RandomString other)
        {
            return !(other is null) && string.Equals(StringValue, other.StringValue);
        }

        /// <summary>
        ///   Determines whether the specified object is equal to the current version.
        /// </summary>
        /// <param name="obj">
        ///   An object to compare to this value.
        /// </param>
        /// <returns>
        ///   <c>true</c> if the specified object is equal to the current version; otherwise <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return !(obj is null) && (obj is RandomString other && Equals(other));
        }

        /// <summary>
        ///   Serves as the default hash function.
        /// </summary>
        /// <returns>
        ///   A hash code for the current value.
        /// </returns>
        public override int GetHashCode()
        {
            return (StringValue != null ? StringValue.GetHashCode() : 0);
        }

        /// <summary>
        ///   Comparison operator overload.
        /// </summary>
        public static bool operator ==(RandomString left, RandomString right)
        {
            return left?.Equals(right) ?? right is null;
        }

        /// <summary>
        ///   Comparison operator overload.
        /// </summary>
        public static bool operator !=(RandomString left, RandomString right)
        {
            return !left?.Equals(right) ?? !(right is null);
        }

        #endregion
    }
}