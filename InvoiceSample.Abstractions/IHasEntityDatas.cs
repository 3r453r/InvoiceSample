using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity
{
    public interface IHasEntityDatas
    {
        IEnumerable<IChildEntityData> Entities { get; }
    }

    public interface IHasEntityData<TEntityData, TKey, TEntityInstanceSelector> : IHasEntityDatas
        where TEntityData : IChildEntityData<TKey, TEntityInstanceSelector>
        where TKey : notnull
        where TEntityInstanceSelector : notnull
    {
        TEntityData? Entity { get; }
    }

    public interface IHasEntityDataCollections
    {
        IEnumerable<(IEnumerable<IChildEntityData> EntityDatas, object Selector)> Collections { get; }
    }

    public interface IHasEntityDataCollection<TEntityData, TKey, TCollectionSelector> : IHasEntityDataCollections
        where TEntityData : IEntityData<TKey>
        where TKey : notnull
        where TCollectionSelector : notnull
    {
        IEnumerable<TEntityData> Entities { get; }
        TCollectionSelector Selector { get; }
    }
}
