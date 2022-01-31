using System;
using System.Linq;

namespace TetraPak.XP.Serialization
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SerializeIgnorePropertiesAttribute : Attribute
    {
        internal string[] PropertyNames { get; }
        
        public SerializeIgnorePropertiesAttribute(params string[] propertyNames)
        {
            PropertyNames = propertyNames.Select(i => i.ToLowerInitial()).ToArray();
        }
    }
}