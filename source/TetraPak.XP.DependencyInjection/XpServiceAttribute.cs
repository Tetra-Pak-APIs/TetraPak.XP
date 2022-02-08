using System;

namespace TetraPak.XP.DependencyInjection
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class XpServiceAttribute : Attribute
    {
        public Type Type { get; }

        public XpServiceAttribute(Type type)
        {
            Type = type;
            XpServices.Register(type, true);
        }
    }
}