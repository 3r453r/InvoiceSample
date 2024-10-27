using InvoiceSample.DataDrivenEntity.Aggregates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity.HasEntityData
{
    public interface IHasMultipleEntityDataInstances<TChildEntityData, TChildKey, TChildSelector> : IAggregateEntityData
        where TChildEntityData : IEntityData<TChildKey>
        where TChildKey : notnull
        where TChildSelector : notnull
    {
        IEnumerable<(TChildEntityData? ChildData, TChildSelector Selector)> ChildInstances { get; }
    }

    public interface IHasMultipleEntityDataInstances<TChildEntityData, TChildKey> : IAggregateEntityData
        where TChildEntityData : IEntityData<TChildKey>
        where TChildKey : notnull
    {
        IEnumerable<(TChildEntityData? ChildData, string Selector)> ChildInstances { get; }
    }
}
