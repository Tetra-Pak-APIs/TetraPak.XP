﻿namespace TetraPak.XP.Scripting
{
    static class ScriptTokens
    {
        // logical 
        internal const char NotOp = '!';
        internal const string AndOp = "&&";
        internal const string OrOp = "||";

        // comparative
        internal const string Equal = "==";
        internal const string NotEqual = "!=";
        internal const string LessThan = "<";               // for strings = "is contained in (but not equal to)"
        internal const string LessThanOrEquals = "<=";      // for strings = "is contained in or is equal"
        internal const string GreaterThan = ">";            // for strings = "contains (but not equal to)"
        internal const string GreaterThanOrEquals = ">=";   // for strings = "contains or is equal"
        // internal const string Contains = "~~";
        internal const string NotContains = "!>";
        internal const string NotContained = "!<";
        
        // misc
        internal const string StringLiteralQualifier = "\"";
        internal const char GroupPrefix = '(';
        internal const char GroupSuffix = ')';

        // numeric
        internal const string Decimal = ".";

        internal static string[] ComparativeOperatorTokens { get; } = 
        {
            Equal,
            NotEqual,
            LessThan,
            LessThanOrEquals,
            GreaterThan,
            GreaterThanOrEquals,
            NotContains,
            NotContained
        };

        public static string ToStringToken(this ScriptLogicOperator op) => op == ScriptLogicOperator.And ? AndOp : OrOp;

        public static ScriptLogicOperator Invert(this ScriptLogicOperator op) => op == ScriptLogicOperator.And ? ScriptLogicOperator.Or : ScriptLogicOperator.And;
        
        public static ComparativeOperation Invert(this ComparativeOperation op) => op == ComparativeOperation.Equal 
            ? ComparativeOperation.NotEqual 
            : ComparativeOperation.Equal;

        public static string ToStringToken(this ComparativeOperation op) => op == ComparativeOperation.Equal ? Equal : NotEqual;

        internal static ComparativeOperation ToOperator(this string token, bool isCollectionOperation)
        {
            return token switch
            {
                Equal => ComparativeOperation.Equal,
                NotEqual => ComparativeOperation.NotEqual,
                LessThan => isCollectionOperation ? ComparativeOperation.Contained : ComparativeOperation.LessThan,
                LessThanOrEquals => isCollectionOperation ? ComparativeOperation.ContainedOrEqual : ComparativeOperation.LessThanOrEquals,
                GreaterThan => isCollectionOperation ? ComparativeOperation.Contains :  ComparativeOperation.GreaterThan,
                GreaterThanOrEquals => isCollectionOperation ? ComparativeOperation.ContainsOrEqual : ComparativeOperation.GreaterThanOrEquals,
                NotContains => ComparativeOperation.NotContains,
                NotContained => ComparativeOperation.NotContained
            };
        }
        
        /*
        internal const string LessThan = "<";               // for strings = "is contained in (but not equal to)"
        internal const string LessThanOrEquals = "<=";      // for strings = "is contained in or is equal"
        internal const string GreaterThan = ">";            // for strings = "contains (but not equal to)"
        internal const string GreaterThanOrEquals = ">=";   // for strings = "contains or is equal"         */

    }
}