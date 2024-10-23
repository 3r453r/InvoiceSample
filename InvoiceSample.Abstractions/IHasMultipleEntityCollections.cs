using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity
{
    public interface IHasMultipleEntityCollections<TChildEntity
        , TSelfEntityData
        , TSelfKey
        , TEntity
        , TChildKey
        , TChildEntityData
        , TCollectionSelector>
    : IHasMultipleEntityCollections
        where TSelfEntityData : IEntityData<TSelfKey>, IHasEntityDataCollections
        where TSelfKey : notnull
        where TChildEntity : IDataDrivenEntity<TEntity, TChildKey, TChildEntityData>, IInitializable
        where TEntity : new()
        where TChildKey : notnull
        where TChildEntityData : IEntityData<TChildKey>
        where TCollectionSelector : notnull
    {
        new Dictionary<TCollectionSelector, ICollection<TChildEntity>> ChildEntityCollections { get; }
    }

    public interface IHasMultipleExternalEntityCollections<TChildEntity
        , TSelfEntityData
        , TSelfKey
        , TEntity
        , TChildKey
        , TChildEntityData
        , TExternalData
        , TCollectionSelector>
    : IHasMultipleEntityCollections
        where TSelfEntityData : IEntityData<TSelfKey>, IHasEntityDataCollections
        where TSelfKey : notnull
        where TChildEntity : IDataDrivenEntity<TEntity, TChildKey, TChildEntityData, TExternalData>, IExternallyInitializable
        where TEntity : new()
        where TChildKey : notnull
        where TChildEntityData : IEntityData<TChildKey>
        where TExternalData : class
        where TCollectionSelector : notnull
    {
        new Dictionary<TCollectionSelector, ICollection<TChildEntity>> ChildEntityCollections { get; }
    }
}
