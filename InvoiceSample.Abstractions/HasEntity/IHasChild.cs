using InvoiceSample.DataDrivenEntity.Aggregates;
using InvoiceSample.DataDrivenEntity.HasEntityData;
using InvoiceSample.DataDrivenEntity.Initializable;

namespace InvoiceSample.DataDrivenEntity.HasEntity
{
    public interface IHasChild<TSelfData
        , TSelfKey
        , TChild
        , TChildKey
        , TChildEntityData
        , TEntity> : IAggregateEntity
        where TSelfData : IEntityData<TSelfKey>, IHasEntityData<TChildEntityData, TChildKey>
        where TSelfKey : notnull
        where TEntity : new()
        where TChildKey : notnull
        where TChildEntityData : IEntityData<TChildKey>
        where TChild : IDataDrivenEntity<TEntity, TChildKey, TChildEntityData>
    {
        TChild? Child { get; set; }
    }

    public interface IHasExternalChild
    {
        object? GetExternalData(IExternalDataDrivenEntity child, IEntityData entityData);
    }

    public interface IHasExternalChild<TSelfData
        , TSelfKey
        , TChild
        , TChildKey
        , TChildEntityData
        , TEntity
        , TExternalData> : IAggregateEntity, IHasExternalChild
        where TSelfData : IEntityData<TSelfKey>, IHasEntityData<TChildEntityData, TChildKey>
        where TSelfKey : notnull
        where TChild : IDataDrivenEntity<TEntity, TChildKey, TChildEntityData, TExternalData>
        where TEntity : new()
        where TChildKey : notnull
        where TChildEntityData : IEntityData<TChildKey>
        where TExternalData : class
    {
        TExternalData? GetExternalData(TSelfData entityData);
        TChildEntityData? GetChildEntityData(TSelfData selfEntityData, TChildKey childKey);
        TChild? Child { get; set; }
    }
}
