using System;
using System.Collections.Generic;
using System.Linq;

namespace TetraPak.XP.Auth.Abstractions;

public sealed class UserInformation
{
    readonly IDictionary<string, object> _dictionary;
    
    public string[] Types => _dictionary.Keys.ToArray();
    
    public bool TryGet<T>(string type, out T? value)
    {
        if (!_dictionary.TryGetValue(type, out var obj))
        {
            value = default;
            return false;
        }
    
        if (obj is not T typedValue) 
            throw new NotImplementedException();
            
        value = typedValue;
        return true;
    
        // todo Cast from Json Token to requested value.
        // todo Also replace Json Token with converted value to avoid converting twice
    }
    
    public UserInformation(IDictionary<string, object> dictionary)
    {
        _dictionary = dictionary;
    }
}