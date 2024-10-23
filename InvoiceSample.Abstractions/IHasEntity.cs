using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity
{
    public interface IHasEntity<TChildEntity
        , TSelfEntityData
        , TSelfKey
        , TEntity
        , TChildKey
        , TChildEntityData
        , TChildInstanceSelector>
        : IHasEntity
        where TSelfEntityData : IEntityData<TSelfKey>, IHasEntityDataCollection<TChildEntityData, TChildKey, TChildInstanceSelector>
        where TSelfKey : notnull
        where TChildEntity : IDataDrivenEntity<TEntity, TChildKey, TChildEntityData>, IInitializable
        where TEntity : new()
        where TChildKey : notnull
        where TChildEntityData : IEntityData<TChildKey>
        where TChildInstanceSelector : notnull
    {
        new TChildEntity ChildEntity { get; }
        TChildEntityData GetChildEntityData(TSelfEntityData entityData, TChildKey childKey);
        new TChildInstanceSelector Selector { get; }
    }

    public interface IHasEntity<TChildEntity, TSelfEntityData, TSelfKey, TEntity, TChildKey, TChildEntityData, TInitializationResult, TChildInstanceSelector>
        : IHasEntityWithResult
        where TSelfEntityData : IEntityData<TSelfKey>, IHasEntityDataCollection<TChildEntityData, TChildKey, TChildInstanceSelector>
        where TSelfKey : notnull
        where TChildEntity : IDataDrivenEntityWithResult<TEntity, TChildKey, TInitializationResult, TChildEntityData>, IInitializableWithResult
        where TEntity : new()
        where TChildKey : notnull
        where TChildEntityData : IEntityData<TChildKey>
        where TInitializationResult : class
        where TChildInstanceSelector : notnull
    {
        new TChildEntity ChildEntity { get; }
        TChildEntityData GetChildEntityData(TSelfEntityData entityData, TChildKey childKey);
    }

    public interface IHasExternalEntity<TSelf, TChildEntity, TSelfEntityData, TSelfKey, TEntity
        , TChildKey, TChildEntityData, TExternalData, TChildInstanceSelector> 
        : IHasExternalEntity
        where TSelfEntityData : IEntityData<TSelfKey>, IHasEntityDataCollection<TChildEntityData, TChildKey, TChildInstanceSelector>
        where TSelfKey : notnull
        where TChildEntity : IDataDrivenEntity<TEntity, TChildKey, TChildEntityData, TExternalData>, IExternallyInitializable
        where TEntity : new()
        where TChildKey : notnull
        where TChildEntityData : IEntityData<TChildKey>
        where TExternalData : class
        where TChildInstanceSelector : notnull
    {
        new TChildEntity ChildEntity { get; }
        TExternalData? GetExternalData(TSelf entity, TSelfEntityData entityData);
        TChildEntityData? GetChildEntityData(TSelfEntityData entityData, TChildKey childKey);
        new TChildInstanceSelector Selector { get; }
    }

    public interface IHasExternalEntity<TSelf, TChildEntity, TSelfEntityData, TSelfKey
        , TEntity, TChildKey, TChildEntityData, TInitializationResult, TExternalData, TChildInstanceSelector>
        : IHasExternalEntityWithResult
        where TSelfEntityData : IEntityData<TSelfKey>, IHasEntityData<TChildEntityData, TChildKey, TChildInstanceSelector>
        where TSelfKey : notnull
        where TChildEntity : IDataDrivenEntityWithResult<TEntity, TChildKey, TInitializationResult, TChildEntityData>, IExternallyInitializableWithResult
        where TEntity : new()
        where TChildKey : notnull
        where TChildEntityData : IChildEntityData<TChildKey, TChildInstanceSelector>
        where TExternalData : class
        where TInitializationResult : class
        where TChildInstanceSelector : notnull
    {
        new TChildEntity ChildEntity { get; }
        TExternalData? GetExternalData(TSelf self, TSelfEntityData selfData);
        TChildEntityData GetChildEntityData(TSelfEntityData entityData, TChildKey childKey);
        new TChildInstanceSelector Selector { get; }
    }
}
