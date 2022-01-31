using System;
using Microsoft.Extensions.Logging;

namespace TetraPak.Logging
{
    /// <summary>
    ///   Add this attribute to properties of classes that are to be included in a "state dump".
    /// </summary>
    /// <seealso cref="WebLoggerHelper.GetStateDump"/>
    /// <seealso cref="StateDumpContext"/>
    /// <seealso cref="RestrictedValueAttribute"/>
    public class StateDumpAttribute : Attribute
    {
    }
}