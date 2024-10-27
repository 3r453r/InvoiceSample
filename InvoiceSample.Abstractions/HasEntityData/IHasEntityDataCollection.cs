using InvoiceSample.DataDrivenEntity.Aggregates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity.HasEntityData
{
    public interface IHasEntityDataCollection<TParentData, TParentKey, TChildData, TChildKey> : IAggregateEntityData
        where TChildData : IEntityData<TChildKey>
        where TChildKey : notnull
        where TParentData : IEntityData<TParentKey>
        where TParentKey : notnull
    {
        IEnumerable<TChildData> GetChildDatas(TParentData parentData);
    }
}
