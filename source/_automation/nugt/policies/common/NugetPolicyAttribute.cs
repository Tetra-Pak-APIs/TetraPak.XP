using System;

namespace nugt.policies
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NugetPolicyAttribute : Attribute
    {
        public string Name { get; }

        public NugetPolicyAttribute(string name)
        {
            Name = name;
        }
    }
}