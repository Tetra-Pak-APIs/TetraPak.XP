using System;
using TetraPak.XP.Logging.Abstractions;

namespace TetraPak.XP.Diagnostics
{
    /// <summary>
    ///   Add this attribute to properties of classes that are to be included in a "state dump".
    /// </summary>
    /// <seealso cref="StateDumpContext"/>
    /// <seealso cref="RestrictedValueAttribute"/>
    public sealed class StateDumpAttribute : Attribute
    {
    }
}