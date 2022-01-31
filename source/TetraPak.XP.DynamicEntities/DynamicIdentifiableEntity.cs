using System;
using System.Diagnostics;
using System.Text.Json.Serialization;
using TetraPak.XP.Serialization;

namespace TetraPak.XP.DynamicEntities
{
    [Serializable, JsonConverter(typeof(DynamicEntityJsonConverter<DynamicEntity>))]
    public abstract class DynamicIdentifiableEntity<TId> : DynamicEntity, IIdProvider<TId>, IUniquelyIdentifiable
    {
        public const string KeyId = "id";
        int _hashCode;
    
        [JsonPropertyName(KeyId)]
        public virtual TId Id
        {
            [DebuggerStepThrough]
            get => GetValue<TId>(KeyId)!;
            set
            {
                SetValue(KeyId, value);
                _hashCode = value?.GetHashCode() ?? throw new ArgumentNullException(nameof(value));
            }
        }

        public override int GetHashCode() => _hashCode; // todo consider splitting serializable entities from "fixed" entities 
        
        public object GetUniqueIdentity() => Id!;

        public virtual TEntity WithId<TEntity>(TId id) where TEntity : DynamicIdentifiableEntity<TId>
        {
            Id = id;
            return (TEntity) this;
        }

        protected DynamicIdentifiableEntity()
        {
        }

        protected DynamicIdentifiableEntity(TId id)
        {
            init(id);
        }
       
        void init(TId id)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
        }
    }
}