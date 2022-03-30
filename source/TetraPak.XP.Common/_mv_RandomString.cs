// using System;
// using System.Diagnostics;
// using System.Text;
//
// namespace TetraPak.XP
// {
//     //[JsonConverter(typeof(JsonStringValueSerializer<RandomString>))]
//     [DebuggerDisplay("{" + nameof(StringValue) + "}")]
//     public class RandomString : IStringValue
//     {
//         static readonly Random s_rnd = new Random(DateTime.Now.Millisecond);
//         static readonly char[] s_lowercase = makeLowercaseLetters();
//         static readonly char[] s_uppercase = makeUppercaseLetters();
//         static readonly char[] s_letters = makeLetters();
//         static readonly char[] s_digits = makeDigits();
//         static readonly char[] s_special = makeSpecial();
//         static readonly char[] s_chars = makeChars(false);
//         static readonly char[] s_charsWithSpecialChars = makeChars(true);
//
//         public static uint DefaultLength { get; set; } = 32;
//
//         static char[] makeLowercaseLetters()
//         {
//             var sb = new StringBuilder();
//             for (var c = 'a'; c <= 'z'; c++) sb.Append(c);
//             return sb.ToString().ToCharArray();
//         }
//         
//         static char[] makeUppercaseLetters()
//         {
//             var sb = new StringBuilder();
//             for (var c = 'A'; c <= 'Z'; c++) sb.Append(c);
//             return sb.ToString().ToCharArray();
//         }
//         
//         static char[] makeLetters() => s_lowercase.Join(s_uppercase);
//
//         static char[] makeDigits()
//         {
//             var sb = new StringBuilder();
//             for (var c = '0'; c <= '9'; c++) sb.Append(c);
//             return sb.ToString().ToCharArray();
//         }
//         
//         static char[] makeSpecial()
//         {
//             var sb = new StringBuilder();
//             sb.Append("-_!#€%&/()?=*+:;<>");
//             return sb.ToString().ToCharArray();
//         }
//         
//         static char[] makeChars(bool allowSpecialCharacters)
//         {
//             var sb = new StringBuilder();
//             sb.Append(makeLowercaseLetters());
//             sb.Append(makeUppercaseLetters());
//             sb.Append(makeDigits());
//             if (allowSpecialCharacters)
//             {
//                 sb.Append(makeSpecial());
//             }
//
//             return sb.ToString().ToCharArray();
//         }
//
//         /// <inheritdoc />
//         public string StringValue { get; }
//
//         public static RandomString Digits(in uint minLength, in uint maxLength)
//         {
//             var length = (uint) s_rnd.Next((int) minLength, (int) maxLength);
//             length = length == 0 ? DefaultLength : length;
//             return Digits(length);
//         }
//
//         public static RandomString Digits(in uint length) => new RandomString(randomString(length, s_digits));
//
//         public static RandomString Letters(in uint minLength, in uint maxLength)
//         {
//             var length = (uint) s_rnd.Next((int) minLength, (int) maxLength);
//             length = length == 0 ? DefaultLength : length;
//             return Letters(length);
//         }
//
//         public static RandomString Letters(in uint length) => new RandomString(randomString(length, s_digits));
//
//         static string randomString(in uint length, char[] chars)
//         {
//             var sb = new StringBuilder();
//             for (var i = 0; i < length; i++)
//             {
//                 var next = s_rnd.Next(chars.Length);
//                 sb.Append(chars[next]);
//             }
//             return sb.ToString();
//         }
//
//         static string randomDigits(in uint length) => randomString(length, s_digits);
//
//         /// <summary>
//         ///   Implicitly converts a <see cref="RandomString"/> value into its <see cref="string"/> representation.
//         /// </summary>
//         /// <param name="value">
//         ///   A <see cref="RandomString"/> value to be implicitly converted into its <see cref="string"/> representation.
//         /// </param>
//         /// <returns>
//         ///   The <see cref="string"/> representation of <paramref name="value"/>.
//         /// </returns>
//         public static implicit operator string?(RandomString? value) => value?.StringValue;
//
//         /// <inheritdoc />
//         public override string ToString() => StringValue;
//
//         #region .  Equality  .
//
//         /// <summary>
//         ///   Determines whether the specified value is equal to the current value.
//         /// </summary>
//         /// <param name="other">
//         ///   A <see cref="RandomString"/> value to compare to this value.
//         /// </param>
//         /// <returns>
//         ///   <c>true</c> if <paramref name="other"/> is equal to the current value; otherwise <c>false</c>.
//         /// </returns>
//         public bool Equals(RandomString? other)
//         {
//             return other is {} && string.Equals(StringValue, other.StringValue);
//         }
//
//         /// <summary>
//         ///   Determines whether the specified object is equal to the current version.
//         /// </summary>
//         /// <param name="obj">
//         ///   An object to compare to this value.
//         /// </param>
//         /// <returns>
//         ///   <c>true</c> if the specified object is equal to the current version; otherwise <c>false</c>.
//         /// </returns>
//         public override bool Equals(object? obj)
//         {
//             return obj is RandomString other && Equals(other);
//         }
//
//         /// <summary>
//         ///   Serves as the default hash function.
//         /// </summary>
//         /// <returns>
//         ///   A hash code for the current value.
//         /// </returns>
//         public override int GetHashCode()
//         {
//             return StringValue != null ? StringValue.GetHashCode() : 0;
//         }
//
//         /// <summary>
//         ///   Comparison operator overload.
//         /// </summary>
//         public static bool operator ==(RandomString? left, RandomString? right)
//         {
//             return left?.Equals(right) ?? right is null;
//         }
//
//         /// <summary>
//         ///   Comparison operator overload.
//         /// </summary>
//         public static bool operator !=(RandomString? left, RandomString? right)
//         {
//             return !left?.Equals(right) ?? right is {};
//         }
//
//         #endregion
//         
//         /// <summary>
//         ///   Initializes the value.
//         /// </summary>
//         /// <param name="length">
//         ///   The length of the random string.
//         /// </param>
//         /// <param name="allowSpecialCharacters">
//         ///   When set, special characters can be included in the random string. 
//         /// </param>
//         [DebuggerStepThrough]
//         public RandomString(uint length = 0, bool allowSpecialCharacters = false)
//         {
//             length = length == 0 ? DefaultLength : length;
//             StringValue = randomString(length, allowSpecialCharacters ? s_charsWithSpecialChars : s_chars);
//         }
//
//         public RandomString(uint minLength, uint maxLength, bool allowSpecialCharacters = false)
//         {
//             var length = (uint) s_rnd.Next((int) minLength, (int) maxLength);
//             length = length == 0 ? DefaultLength : length;
//             StringValue = randomString(length, allowSpecialCharacters ? s_charsWithSpecialChars : s_chars);
//         }
//
//         RandomString(string stringValue)
//         {
//             StringValue = stringValue;
//         }
//     }
// }