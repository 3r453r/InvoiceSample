using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity
{
    public interface IHasMultipleEntityInstances<TChildEntity
        , TSelfEntityData
        , TSelfKey
        , TEntity
        , TChildKey
        , TChildEntityData
        , TChildInstanceSelector>
    : IHasMultipleEntityInstances
        where TSelfEntityData : IEntityData<TSelfKey>, IHasEntityDatas
        where TSelfKey : notnull
        where TChildEntity : IDataDrivenEntity<TEntity, TChildKey, TChildEntityData>, IInitializable
        where TEntity : new()
        where TChildKey : notnull
        where TChildEntityData : IEntityData<TChildKey>
        where TChildInstanceSelector : notnull
    {
        new Dictionary<TChildInstanceSelector, TChildEntity?> ChildEntityInstances { get; }
    }

    public interface IHasMultipleExternalEntityInstances<TChildEntity
        , TSelfEntityData
        , TSelfKey
        , TEntity
        , TChildKey
        , TChildEntityData
        , TExternalData
        , TChildInstanceSelector>
    : IHasMultipleEntityInstances
        where TSelfEntityData : IEntityData<TSelfKey>, IHasEntityDatas
        where TSelfKey : notnull
        where TChildEntity : IDataDrivenEntity<TEntity, TChildKey, TChildEntityData, TExternalData>, IExternallyInitializable
        where TEntity : new()
        where TChildKey : notnull
        where TChildEntityData : IEntityData<TChildKey>
        where TChildInstanceSelector : notnull
        where TExternalData : class
    {
        new Dictionary<TChildInstanceSelector, TChildEntity?> ChildEntityInstances { get; }
    }
}
