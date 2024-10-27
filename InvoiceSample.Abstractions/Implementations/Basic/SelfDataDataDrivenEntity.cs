using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity.Implementations.Basic
{
    public abstract class SelfDataDataDrivenEntity<TEntity, TKey, TEntityData> : DataDrivenEntity<TEntity, TKey, TEntityData>
        where TEntity : TEntityData, new()
        where TEntityData : IEntityData<TKey>
        where TKey : notnull
    {
        protected SelfDataDataDrivenEntity()
        {
        }

        public override TEntityData GetEntityData()
        {
            if(this is TEntityData data)
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
    }
}
