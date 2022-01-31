using System;
using System.Diagnostics;

#nullable enable

namespace TetraPak.XP
{
    /// <summary>
    ///   Can be used to manage indentation while building textual values.
    /// </summary>
    [DebuggerDisplay("{ToDebugString()}")]
    public readonly struct Indentation : IStringValue
    {
        /// <summary>
        ///   The fixed number of (whitespace) characters in use. 
        /// </summary>
        /// <seealso cref="Push"/>
        /// <seealso cref="Pop"/>
        public int Increment { get; }

        /// <summary>
        ///   The textual indentation in used.
        /// </summary>
        public string StringValue { get; }

        /// <summary>
        ///   Gets the current indentation length.
        /// </summary>
        public int Length => StringValue.Length;

        /// <summary>
        ///   Produces and returns a new ("lower") indentation level based on <c>this</c> one.
        /// </summary>
        /// <returns>
        ///   A new <see cref="Indentation"/> value.
        /// </returns>
        public Indentation Push() => new(Increment, StringValue[0], StringValue.Length + Increment);

        /// <summary>
        ///   Returns the previous indentation level (if any).
        /// </summary>
        public Indentation Pop() => new(Increment, StringValue[0], StringValue.Length - Increment);

        public override string ToString() => StringValue;

        internal string ToDebugString() => $"{Length} ({Increment})";

        public static implicit operator string(Indentation indentation) => indentation.StringValue;
        
        /// <summary>
        ///   Initializes the <see cref="Indentation"/>.
        /// </summary>
        /// <param name="increment">
        ///   Initializes <see cref="Increment"/>.
        /// </param>
        /// <param name="character">
        ///   (optional; default=' ' [space])<br/>
        ///   Specifies the character used to build the textual indentation (<see cref="StringValue"/>).
        /// </param>
        public Indentation(int increment, char character = ' ')
        : this(increment, character, increment/*, null obsolete */)
        {
        }

        Indentation(int increment, char character, int currentLength/*, Indentation? previous obsolete */)
        {
            Increment = increment;
            currentLength = Math.Max(0, currentLength);
            StringValue = new string(character, currentLength);
            // _previous = previous; obsolete
        }

    }
}