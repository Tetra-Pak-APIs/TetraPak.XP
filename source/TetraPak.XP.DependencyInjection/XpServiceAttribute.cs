using System;

namespace TetraPak.XP.DependencyInjection
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class XpServiceAttribute : Attribute
    {
        public XpServiceAttribute(Type type)
        {
            XpServices.Register(type, false);
        }
        
        public XpServiceAttribute(Type implementsInterface, Type type)
        {
            if (!implementsInterface.IsInterface)
                throw new ArgumentException($"{nameof(implementsInterface)} type must be interface", nameof(implementsInterface));
            
            if (!type.IsImplementingInterface(implementsInterface))
                throw new InvalidOperationException($"{type} is not implementing interface {implementsInterface}");

            XpServices.Register(type, implementsInterface, false);
        }
    }
}