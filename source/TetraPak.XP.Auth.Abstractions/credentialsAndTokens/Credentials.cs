using System;
using System.Text.Json.Serialization;
using TetraPak.XP.DynamicEntities;
using TetraPak.XP.Logging;

namespace TetraPak.XP.Auth.Abstractions
{
    /// <summary>
    ///   Represents a generic credentials value, typically used for authentication use purposes.
    /// </summary>
    //[JsonConverter(typeof(DynamicEntityJsonConverter<Credentials>))]
    public class Credentials : DynamicIdentifiableEntity<string>, IDisposable
    {
        const string KeySecret = "secret";
        const string KeyNewSecret = "newSecret";
        
        /// <summary>
        ///   Gets or sets the credentials identity element.
        /// </summary>
        [JsonIgnore]
        [StateDump]
        public string Identity
        {
            get => Id;
            set => Id = value;
        }

        /// <summary>
        ///   Gets or sets the credentials secret/password element.
        /// </summary>
        [JsonPropertyName(KeySecret)]
        [StateDump, RestrictedValue(DisclosureLogLevel = LogRank.Debug)]
        public string? Secret
        {
            get => GetValue<string>(KeySecret);
            set => SetValue(KeySecret, value);
        }

        /// <summary>
        ///   Gets or sets a new credentials secret/password element.
        /// </summary>
        [JsonPropertyName(KeyNewSecret)]
        [StateDump, RestrictedValue(DisclosureLogLevel = LogRank.Debug)]
        public string? NewSecret
        {
            get => GetValue<string>(KeyNewSecret);
            set => SetValue(KeyNewSecret, value);
        }

        /// <summary>
        ///   Clones the entity without any secrets, to support scenarios where secrets are not necessary.
        /// </summary>
        /// <returns>
        ///   A cloned <see cref="Credentials"/>.
        /// </returns>
        public Credentials CloneWithoutSecrets()
        {
            var clone = Clone<Credentials>();
            clone.SetValue<string>(KeySecret, null!);
            clone.SetValue<string>(KeyNewSecret, null!);
            return clone;
        }

        /// <summary>
        ///   Gets a value indicating whether the credentials are assigned.
        /// </summary>
        public virtual bool IsAssigned => !string.IsNullOrWhiteSpace(Identity) && !string.IsNullOrWhiteSpace(Secret);

        /// <summary>
        ///   Initializes the <see cref="Credentials"/> value.
        /// </summary>
#if NET5_0_OR_GREATER            
        [JsonConstructor]
#endif
        public Credentials()
        {
        }


        /// <summary>
        ///   Initializes the <see cref="Credentials"/> value.
        /// </summary>
        /// <param name="identity">
        ///   Initializes the <see cref="Identity"/> property.
        /// </param>
        /// <param name="secret">
        ///   Initializes the <see cref="Secret"/> property.
        /// </param>
        /// <param name="newSecret">
        ///   (optional)<br/>
        ///   Initializes the <see cref="NewSecret"/> property.
        /// </param>
        public Credentials(string identity, string? secret, string? newSecret = null) 
        : base(identity)
        {
            Secret = secret;
            NewSecret = newSecret;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }
        
        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}