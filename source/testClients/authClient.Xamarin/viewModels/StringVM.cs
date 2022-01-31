using System;
using TetraPak.XP;
using TetraPak.XP.Logging;

namespace authClient.viewModels
{
    public class StringVM : ValidatedValueVM<string>
    {
        protected override Outcome<string> OnValidateValue(string? value)
        {
            value = value?.Trim();
            if (string.IsNullOrEmpty(value) && IsRequired)
                return Outcome<string>.Fail(new ArgumentNullException(nameof(value), "This value is required"));

            return Outcome<string>.Success(value!);
        }

        public StringVM(string valueName, ViewModel parent, IServiceProvider services, ILog? log)
        : base(valueName, parent, services, log)
        {
        }
    }
}