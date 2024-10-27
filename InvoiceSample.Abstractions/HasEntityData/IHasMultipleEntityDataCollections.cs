using InvoiceSample.DataDrivenEntity.Aggregates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity.HasEntityData
{
    public interface IHasMultipleEntityDataCollections<TChildData, TChildKey, TCollectionSelector> : IAggregateEntityData
        where TChildData : IEntityData<TChildKey>
        where TChildKey : notnull
        where TCollectionSelector : notnull
    {
        IEnumerable<(IEnumerable<TChildData> Collection, TCollectionSelector Selector)> ChildrenCollections { get; }
    }

    public interface IHasMultipleEntityDataCollections<TChildData, TChildKey> : IAggregateEntityData
        where TChildData : IEntityData<TChildKey>
        where TChildKey : notnull
    {
        IEnumerable<(IEnumerable<TChildData> Collection, string Selector)> ChildrenCollections { get; }
    }
}
