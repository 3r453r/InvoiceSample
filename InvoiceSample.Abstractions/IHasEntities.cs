using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity
{
    public interface IHasEntities<TChildEntity, TSelfEntityData, TSelfKey, TEntity
        , TChildKey, TChildEntityData, TCollectionSelector>
    : IHasEntities
    where TSelfEntityData : IEntityData<TSelfKey>, IHasEntityDataCollection<TChildEntityData, TChildKey, TCollectionSelector>
    where TSelfKey : notnull
    where TChildEntity : IDataDrivenEntity<TEntity, TChildKey, TChildEntityData>, IInitializable
    where TEntity : new()
    where TChildKey : notnull
    where TChildEntityData : IEntityData<TChildKey>
        where TCollectionSelector : notnull
    {
        new ICollection<TChildEntity> ChildEntities { get; }
        IEnumerable<TChildEntityData> GetChildrenEntityData(TSelfEntityData entityData);
        new TCollectionSelector Selector { get; }
    }

    public interface IHasEntities<TChildEntity, TSelfEntityData, TSelfKey
        , TEntity, TChildKey, TChildEntityData, TInitializationResult, TCollectionSelector>
    : IHasEntitiesWithResult
    where TSelfEntityData : IEntityData<TSelfKey>, IHasEntityDataCollection<TChildEntityData, TChildKey, TCollectionSelector>
    where TSelfKey : notnull
    where TChildEntity : IDataDrivenEntityWithResult<TEntity, TChildKey, TInitializationResult, TChildEntityData>, IInitializableWithResult
    where TEntity : new()
    where TChildKey : notnull
    where TChildEntityData : IEntityData<TChildKey>
    where TInitializationResult : class
        where TCollectionSelector : notnull
    {
        new ICollection<TChildEntity> ChildEntities { get; }
        new TCollectionSelector Selector { get; }
    }

    public interface IHasExternalEntities<TSelf, TChildEntity, TSelfEntityData
        , TSelfKey, TEntity, TChildKey, TChildEntityData, TExternalData, TCollectionSelector>
    : IHasExternalEntities
    where TSelfEntityData : IEntityData<TSelfKey>, IHasEntityDataCollection<TChildEntityData, TChildKey, TCollectionSelector>
    where TSelfKey : notnull
    where TChildEntity : IDataDrivenEntity<TEntity, TChildKey, TChildEntityData, TExternalData>, IExternallyInitializable
    where TEntity : new()
    where TChildKey : notnull
    where TChildEntityData : IEntityData<TChildKey>
    where TExternalData : class
        where TCollectionSelector : notnull
    {
        new ICollection<TChildEntity> ChildEntities { get; }
        TExternalData? GetExternalData(TSelf self, TSelfEntityData selfData);
        new TCollectionSelector Selector { get; }
    }

    public interface IHasExternalEntities<TSelf, TChildEntity, TSelfEntityData
        , TSelfKey, TEntity, TChildKey, TChildEntityData, TInitializationResult, TExternalData, TCollectionSelector>
    : IHasExternalEntitiesWithResult
    where TSelfEntityData : IEntityData<TSelfKey>, IHasEntityDataCollection<TChildEntityData, TChildKey, TCollectionSelector>
    where TSelfKey : notnull
    where TChildEntity : IDataDrivenEntityWithResult<TEntity, TChildKey, TInitializationResult, TChildEntityData>, IExternallyInitializableWithResult
    where TEntity : new()
    where TChildKey : notnull
    where TChildEntityData : IEntityData<TChildKey>
    where TExternalData : class
    where TInitializationResult : class
        where TCollectionSelector : notnull
    {
        new ICollection<TChildEntity> ChildEntities { get; }
        TExternalData? GetExternalData(TSelf self, TSelfEntityData selfData);
        new TCollectionSelector Selector { get; }
    }
}
