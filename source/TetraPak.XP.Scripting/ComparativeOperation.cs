using System;

namespace TetraPak.XP.Scripting;

/// <summary>
///   used to express a comparison operation.
/// </summary>
public enum ComparativeOperation
{
    /// <summary>
    ///   No (recognized) comparison operation is expressed. 
    /// </summary>
    None,
    
    /// <summary>
    ///   Specifies the "is equal" operation.
    /// </summary>
    Equal,
    
    /// <summary>
    ///   Specifies the "is not equal" operation.
    /// </summary>
    NotEqual,
    
    /// <summary>
    ///   Specifies the "is less than" operation.
    /// </summary>
    LessThan,

    /// <summary>
    ///   Specifies the "is less than, or equal to" operation.
    /// </summary>
    LessThanOrEquals,
    
    /// <summary>
    ///   Specifies the "is greater than" operation.
    /// </summary>
    GreaterThan,

    /// <summary>
    ///   Specifies the "is greater than, or equal to" operation.
    /// </summary>
    GreaterThanOrEquals,

    /// <summary>
    ///   Specifies the "contains" operation.
    /// </summary>
    Contains,
    
    /// <summary>
    ///   Specifies the "contains, or equal to" operation.
    /// </summary>
    ContainsOrEqual,

    /// <summary>
    ///   Specifies the "not contains" operation.
    /// </summary>
    NotContains,
    
    /// <summary>
    ///   Specifies the "is contained in" operation.
    /// </summary>
    Contained,
    
    /// <summary>
    ///   Specifies the "is contained in" operation.
    /// </summary>
    NotContained,
    
    /// <summary>
    ///   Specifies the "is contained in, or equal to" operation.
    /// </summary>
    ContainedOrEqual,
}

public static class ComparativeOperationHelper
{
    public static ComparativeOperation ToComparativeOperator(this string token)
    {
        return token switch
        {
            ScriptTokens.Equal => ComparativeOperation.Equal,
            ScriptTokens.NotEqual => ComparativeOperation.NotEqual,
            ScriptTokens.LessThan => ComparativeOperation.LessThan,
            ScriptTokens.LessThanOrEquals => ComparativeOperation.LessThanOrEquals,
            ScriptTokens.GreaterThan => ComparativeOperation.GreaterThan,
            ScriptTokens.GreaterThanOrEquals => ComparativeOperation.GreaterThanOrEquals,
            ScriptTokens.Contains => ComparativeOperation.Contains,
            ScriptTokens.NotContains => ComparativeOperation.NotContains,
            ScriptTokens.Contained => ComparativeOperation.Contained,
            ScriptTokens.NotContained => ComparativeOperation.NotContained,
            _ => throw new ArgumentOutOfRangeException(nameof(token), token, null)
        };
    }
    
    internal static ComparativeOperation FromRightOperandPerspective(this ComparativeOperation operation)
    {
        return operation switch
        {
            ComparativeOperation.None => ComparativeOperation.None,
            ComparativeOperation.Equal => ComparativeOperation.Equal,
            ComparativeOperation.NotEqual => ComparativeOperation.NotEqual,
            ComparativeOperation.LessThan => ComparativeOperation.GreaterThan,
            ComparativeOperation.LessThanOrEquals => ComparativeOperation.GreaterThanOrEquals,
            ComparativeOperation.GreaterThan => ComparativeOperation.LessThan,
            ComparativeOperation.GreaterThanOrEquals => ComparativeOperation.LessThanOrEquals,
            ComparativeOperation.Contains => ComparativeOperation.Contained,
            ComparativeOperation.ContainsOrEqual => ComparativeOperation.ContainedOrEqual,
            ComparativeOperation.NotContains => ComparativeOperation.NotContained,
            ComparativeOperation.Contained => ComparativeOperation.Contains,
            ComparativeOperation.NotContained => ComparativeOperation.NotContains,
            ComparativeOperation.ContainedOrEqual => ComparativeOperation.ContainsOrEqual,
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
        };
    }
} 