using System.Reflection;

namespace TetraPak
{
    public static class ReflectionHelper
    {
        /// <summary>
        ///   Returns a value indicating whether a <see cref="PropertyInfo"/> represents
        ///   an indexed property.
        /// </summary>
        public static bool IsIndexer(this PropertyInfo self) => self.GetIndexParameters().Length > 0;        
    }
}