using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TetraPak.XP;

namespace nugt.policies
{
    public class PolicyDispatcher
    {
        readonly Dictionary<string, Type> _policyTypes;

        public Outcome<Type> GetPolicyType(string name)
        {
            return _policyTypes.TryGetValue(name.ToLowerInvariant(), out var type)
                ? Outcome<Type>.Success(type)
                : Outcome<Type>.Fail($"Policy not found: \"{name}\"");
        }

        Dictionary<string, Type> findPolicies()
        {
            var policyTypes = GetType().Assembly.GetTypes()
                .Where(t => !t.IsAbstract && typeof(Policy).IsAssignableFrom(t)).ToArray();

            var types = new Dictionary<string, Type>();
            for (var i = 0; i < policyTypes.Length; i++)
            {
                var attribute = policyTypes[i].GetCustomAttribute<NugetPolicyAttribute>();
                if (attribute is null)
                    throw new Exception($"Nuget policy needs a {typeof(NugetPolicyAttribute)}: {policyTypes[i]}");
                
                types.Add(attribute.Name.ToLowerInvariant(), policyTypes[i]);
            }

            return types;
        }

        public PolicyDispatcher()
        {
            _policyTypes = findPolicies();
        }
    }
}