using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity.Implementations.External
{
    public abstract class SelfDataExternalDataDrivenEntity<TEntity, TKey, TEntityData, TExternalData>
    : ExternalDataDrivenEntity<TEntity, TKey, TEntityData, TExternalData>
        where TEntity : TEntityData, new()
        where TEntityData : IEntityData<TKey>
        where TKey : notnull
        where TExternalData : class
    {
        protected SelfDataExternalDataDrivenEntity()
        {
        }

        public override TEntityData GetEntityData()
        {
            if (this is TEntityData data)
            {
                return data;
            }
            else
            {
                throw new InvalidCastException($"expecting {GetType().Name} to be of type {typeof(TEntity).Name}");
            }
        }

        public override TKey GetKey()
        {
            return ((IEntityData<TKey>)this).GetKey();
        }

        protected override void SelfInitialize(TEntityData entityData, TExternalData externalData)
        {
            Map(entityData);
            InitializeWithExternalData(externalData);
        }

        protected abstract void Map(TEntityData entityData);

        protected abstract void InitializeWithExternalData(TExternalData externalData);
    }
}
