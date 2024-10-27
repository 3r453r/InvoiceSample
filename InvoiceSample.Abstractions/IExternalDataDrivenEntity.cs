using InvoiceSample.DataDrivenEntity.Initializable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity
{
    public interface IExternalDataDrivenEntity : IExternallyInitializable
    {
        object GetEntityData();
        object? GetExternalData(object entityData);
    }

    public interface IDataDrivenEntity<TSelf, TKey, TEntityData, TExternalData> : IExternalDataDrivenEntity
    where TEntityData : IEntityData<TKey>
    where TSelf : new()
    where TKey : notnull
    where TExternalData : class
    {
        void Initialize(TEntityData entityData, TExternalData externalData);
        new TEntityData GetEntityData();
        TExternalData? GetExternalData(TEntityData entityData);
    }

    public interface IDataDrivenEntityWithResult<TEntity, TKey, TEntityData, TExternalData, TInitializationResult>
        : IDataDrivenEntity<TEntity, TKey, TEntityData, TExternalData>, IExternallyInitializableWithResult
        where TInitializationResult : class
        where TEntityData : IEntityData<TKey>
        where TEntity : new()
        where TKey : notnull
    where TExternalData : class
    {
        TInitializationResult InitializeWithResult(TEntityData entityData, TExternalData externalData);
    }
}
