using System;

namespace TetraPak.XP.Serialization
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ImplementedAsAttribute : Attribute
    {
        public Type Type { get; }

        public ImplementedAsAttribute(Type type)
        {
            Type = type;
        }
    }
}