using System;
using System.Collections;
using TetraPak.XP;
using TetraPak.XP.Logging;

namespace authClient.viewModels
{
    /// <summary>
    ///   Represents a value that gets automatically validated when changed.
    /// </summary>
    /// <typeparam name="T">
    ///   The value type.  
    /// </typeparam>
    public class ValidatedValueVM<T> : ValidatedValueVM
    {
        T _value;
        T _placeholderValue;

        public override bool IsRequired
        {
            get => base.IsRequired;
            set
            {
                base.IsRequired = value;
                var isValid = OnValidateValue(Value);
                if (!isValid)
                {
                    IsInvalid = false;
                    Message = isValid.Message;
                }

                IsInvalid = false;
            }
        }

        /// <summary>
        ///   Gets or sets the validated value.
        /// </summary>
        public T Value
        {
            get => _value;
            set
            {
                var ok = OnValidateValue(value);
                SetValue(ref _value, ok.Value);
                if (!ok)
                {
                    IsInvalid = true;
                    Message = ok.Message;
                    return;
                }

                var oldValue = _value;
                IsInvalid = false;
                ParentViewModel?.NotifyChildValueChanged(this, ValueName, oldValue, value);
            }
        }

        /// <summary>
        ///   Gets or sets a 
        /// </summary>
        public T PlaceholderValue
        {
            get => _placeholderValue;
            set => SetValue(ref _placeholderValue, value);
        }
        
        /// <summary>
        ///   This method is automatically invoked to resolve the validity of a
        ///   new value.
        /// </summary>
        /// <param name="value">
        ///   The value to be validated.
        /// </param>
        /// <returns>
        ///   A <see cref="Outcome"/> to indicate validation success/failure.
        /// </returns>
        protected virtual Outcome<T> OnValidateValue(T value)
        {
            var type = typeof(T);
            if (!type.IsValueType)
                return ReferenceEquals(null, value) ? failValueIsRequired() : Outcome<T>.Success(value);

            if (!IsRequired)
                return Outcome<T>.Success(value);

            if (ReferenceEquals(null, value))
                return failValueIsRequired();

            if (value is IEnumerable enumerable && !enumerable.GetEnumerator().MoveNext())
                return failValueIsRequired();

            return Outcome<T>.Success(value);

            Outcome<T> failValueIsRequired() => Outcome<T>.Fail(new Exception($"{ValueName} is required"));
        }

        public override void AssignFrom(ValidatedValueAttribute attribute)
        {
            base.AssignFrom(attribute);
            PlaceholderValue = OnConvertPlaceholderValue(attribute.PlaceholderValue);
        }

        protected virtual T OnConvertPlaceholderValue(object value)
        {
            return (T)value;
        }

        public ValidatedValueVM(string valueName, ViewModel parent, IServiceProvider services, ILog? log) 
        : base(valueName, parent, services, log)
        {
        }
    }

    public abstract class ValidatedValueVM : ViewModel
    {
        bool _isInvalid;
        string _message;
        bool _isRequired;

        public ViewModel ParentViewModel { get; }

        public string ValueName { get; }

        /// <summary>
        ///   Gets a value that indicates whether the <see cref="Value"/> is valid. 
        /// </summary>
        public bool IsInvalid
        {
            get => _isInvalid;
            protected set => SetValue(ref _isInvalid, value);
        }

        /// <summary>
        ///   Gets or sets a value indicating whether the value is required.
        /// </summary>
        public virtual bool IsRequired
        {
            get => _isRequired;
            set => _isRequired = value;
        }

        /// <summary>
        ///   Holds a message (is typically set at validation).
        /// </summary>
        public string Message
        {
            get => _message;
            set => SetValue(ref _message, value);
        }

        public virtual void AssignFrom(ValidatedValueAttribute attribute)
        {
            IsRequired = attribute.IsRequired;
        }

        public ValidatedValueVM(string valueName, ViewModel parent, IServiceProvider services, ILog? log) 
        : base(services, log)
        {
            ValueName = valueName;
            ParentViewModel = parent;
        }
    }
}