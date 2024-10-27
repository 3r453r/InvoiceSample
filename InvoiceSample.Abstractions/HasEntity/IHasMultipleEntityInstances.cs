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
    public interface IHasMultipleEntityInstances<TSelfData
        , TSelfKey
        , TChild
        , TChildKey
        , TChildEntityData
        , TEntity
        , TChildInstanceSelector>
    : IAggregateEntity
        where TSelfData : IEntityData<TSelfKey>, IHasMultipleEntityDataInstances<TChildEntityData, TChildKey, TChildInstanceSelector>
        where TSelfKey : notnull
        where TChild : IDataDrivenEntity<TEntity, TChildKey, TChildEntityData>, IInitializable
        where TEntity : new()
        where TChildKey : notnull
        where TChildEntityData : IEntityData<TChildKey>
        where TChildInstanceSelector : notnull
    {
        IEnumerable<(TChild? Entity, TChildInstanceSelector Selector)> ChildInstances { get; }
    }

    public interface IHasMultipleEntityInstances<TSelfData
        , TSelfKey
        , TChild
        , TChildKey
        , TChildEntityData
        , TEntity>
    : IAggregateEntity
        where TSelfData : IEntityData<TSelfKey>, IHasMultipleEntityDataInstances<TChildEntityData, TChildKey>
        where TSelfKey : notnull
        where TChild : IDataDrivenEntity<TEntity, TChildKey, TChildEntityData>, IInitializable
        where TEntity : new()
        where TChildKey : notnull
        where TChildEntityData : IEntityData<TChildKey>
    {
        IEnumerable<(TChild? Entity, string Selector)> ChildInstances { get; }
    }

    public interface IHasMultipleExternalEntityInstances<TSelfData
        , TSelfKey
        , TChild
        , TChildKey
        , TChildEntityData
        , TEntity
        , TExternalData
        , TChildInstanceSelector>
    : IAggregateEntity
        where TSelfData : IEntityData<TSelfKey>, IHasMultipleEntityDataInstances<TChildEntityData, TChildKey, TChildInstanceSelector>
        where TSelfKey : notnull
        where TChild : IDataDrivenEntity<TEntity, TChildKey, TChildEntityData, TExternalData>, IExternallyInitializable
        where TEntity : new()
        where TChildKey : notnull
        where TChildEntityData : IEntityData<TChildKey>
        where TChildInstanceSelector : notnull
        where TExternalData : class
    {
        IEnumerable<(TChild? Entity, TChildInstanceSelector Selector)> ChildInstances { get; }
    }
}
