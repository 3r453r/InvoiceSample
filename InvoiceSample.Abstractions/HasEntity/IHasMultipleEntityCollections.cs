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
    public interface IHasMultipleEntityCollections<TSelfData
        , TSelfKey
        , TChild
        , TChildKey
        , TChildEntityData
        , TEntity
        , TCollectionSelector>
    : IAggregateEntity
        where TSelfData : IEntityData<TSelfKey>, IHasMultipleEntityDataCollections<TChildEntityData, TChildKey, TCollectionSelector>
        where TSelfKey : notnull
        where TChild : IDataDrivenEntity<TEntity, TChildKey, TChildEntityData>
        where TEntity : new()
        where TChildKey : notnull
        where TChildEntityData : IEntityData<TChildKey>
        where TCollectionSelector : notnull
    {
        IEnumerable<(ICollection<TChild> Collection, TCollectionSelector Selector)> ChildEntityCollections { get; }
    }

    public interface IHasMultipleEntityCollections<TSelfData
        , TSelfKey
        , TChild
        , TChildKey
        , TChildEntityData
        , TEntity>
    : IAggregateEntity
        where TSelfData : IEntityData<TSelfKey>, IHasMultipleEntityDataCollections<TChildEntityData, TChildKey, string>
        where TSelfKey : notnull
        where TChild : IDataDrivenEntity<TEntity, TChildKey, TChildEntityData>
        where TEntity : new()
        where TChildKey : notnull
        where TChildEntityData : IEntityData<TChildKey>
    {
        IEnumerable<(ICollection<TChild> Collection, string Selector)> ChildEntityCollections { get; }
    }

    public interface IHasMultipleExternalEntityCollections<TSelfData
        , TSelfKey
        , TChild
        , TChildKey
        , TChildEntityData
        , TEntity
        , TExternalData
        , TCollectionSelector>
    : IAggregateEntity, IHasExternalChild
        where TSelfData : IEntityData<TSelfKey>, IHasMultipleEntityDataCollections<TChildEntityData, TChildKey, TCollectionSelector>
        where TSelfKey : notnull
        where TChild : IDataDrivenEntity<TEntity, TChildKey, TChildEntityData, TExternalData>, IExternallyInitializable
        where TEntity : new()
        where TChildKey : notnull
        where TChildEntityData : IEntityData<TChildKey>
        where TExternalData : class
        where TCollectionSelector : notnull
    {
        IEnumerable<(ICollection<TChild> Collection, TCollectionSelector Selector)> ChildEntityCollections { get; }
    }

    public interface IHasMultipleExternalEntityCollections<TSelfData
        , TSelfKey
        , TChild
        , TChildKey
        , TChildEntityData
        , TEntity
        , TExternalData>
    : IAggregateEntity, IHasExternalChild
        where TSelfData : IEntityData<TSelfKey>, IHasMultipleEntityDataCollections<TChildEntityData, TChildKey>
        where TSelfKey : notnull
        where TChild : IDataDrivenEntity<TEntity, TChildKey, TChildEntityData, TExternalData>, IExternallyInitializable
        where TEntity : new()
        where TChildKey : notnull
        where TChildEntityData : IEntityData<TChildKey>
        where TExternalData : class
    {
        IEnumerable<(ICollection<TChild> Collection, string Selector)> ChildEntityCollections { get; }
    }
}
