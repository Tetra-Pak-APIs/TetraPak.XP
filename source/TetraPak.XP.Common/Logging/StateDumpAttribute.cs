﻿using System;

namespace TetraPak.XP.Logging
{
    /// <summary>
    ///   Add this attribute to properties of classes that are to be included in a "state dump".
    /// </summary>
    /// <seealso cref="StateDumpContext"/>
    /// <seealso cref="RestrictedValueAttribute"/>
    public class StateDumpAttribute : Attribute
    {
    }
}