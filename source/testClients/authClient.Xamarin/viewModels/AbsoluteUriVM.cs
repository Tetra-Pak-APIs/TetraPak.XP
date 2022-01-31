using System;
using TetraPak.XP;
using TetraPak.XP.Logging;

namespace authClient.viewModels
{
    /// <summary>
    ///   An absolute URI value that works well in view models.
    /// </summary>
    public class AbsoluteUriVM : StringVM
    {
        public Uri UriValue => IsInvalid ? null : new Uri(Value);

        protected override Outcome<string> OnValidateValue(string value)
        {
            value = value?.Trim();
            if (string.IsNullOrEmpty(value))
                return base.OnValidateValue(value);

            return !Uri.TryCreate(value, UriKind.Absolute, out _) 
                ? Outcome<string>.Fail(new FormatException("Invalid absolute URI")) 
                : Outcome<string>.Success(value);
        }
        
        public AbsoluteUriVM(string valueName, ViewModel parent, IServiceProvider services, ILog log)
        : base(valueName, parent, services, log)
        {
        }
    }
}