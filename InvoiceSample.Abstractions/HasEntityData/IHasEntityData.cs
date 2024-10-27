using InvoiceSample.DataDrivenEntity.Aggregates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity.HasEntityData
{
    public interface IHasEntityData<TParentData, TParentKey, TChildEntityData, TChildKey> : IAggregateEntityData
        where TChildEntityData : IEntityData<TChildKey>
        where TChildKey : notnull
        where TParentData : IEntityData<TParentKey>
        where TParentKey : notnull
    {
        TChildEntityData? GetChildEntityData(TParentData parentEntityData);
    }
}
