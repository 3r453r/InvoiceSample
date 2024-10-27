using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InvoiceSample.DataDrivenEntity.Aggregates;
using InvoiceSample.DataDrivenEntity.HasEntityData;
using InvoiceSample.DataDrivenEntity.Initializable;

namespace InvoiceSample.DataDrivenEntity.HasEntity
{
    public interface IHasChildren<TSelfData
        , TSelfKey
        , TChild
        , TChildKey
        , TChildEntityData
        , TEntity> : IAggregateEntity
        where TSelfData : IEntityData<TSelfKey>, IHasEntityDataCollection<TChildEntityData, TChildKey>
        where TSelfKey : notnull
        where TChild : IDataDrivenEntity<TEntity, TChildKey, TChildEntityData>
        where TEntity : new()
        where TChildKey : notnull
        where TChildEntityData : IEntityData<TChildKey>
    {
        ICollection<TChild> ChildEntities { get; }
        IEnumerable<TChildEntityData> GetChildrenEntityData(TSelfData entityData);
    }

    public interface IHasExternalChildren<TSelfData
        , TSelfKey
        , TChild
        , TChildKey
        , TChildEntityData
        , TEntity
        , TExternalData>
    : IAggregateEntity, IHasExternalChild
    where TSelfData : IEntityData<TSelfKey>
    where TSelfKey : notnull
    where TChild : IDataDrivenEntity<TEntity, TChildKey, TChildEntityData, TExternalData>
    where TEntity : new()
    where TChildKey : notnull
    where TChildEntityData : IEntityData<TChildKey>
    where TExternalData : class
    {
        ICollection<TChild> ChildEntities { get; }
        IEnumerable<TChildEntityData> GetChildrenEntityData(TSelfData entityData);
        TExternalData? GetExternalData(TSelfData selfData);
    }
}
