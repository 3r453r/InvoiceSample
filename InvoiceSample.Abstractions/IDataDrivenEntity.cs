using InvoiceSample.DataDrivenEntity.Initializable;

namespace InvoiceSample.DataDrivenEntity
{
    public interface IDataDrivenEntity : IInitializable
    {
        object GetEntityData();
    }

    public interface IDataDrivenEntity<TSelf, TKey, TEntityData> : IDataDrivenEntity
        where TSelf : new()
        where TEntityData : IEntityData<TKey>
        where TKey : notnull
    {
        void Initialize(TEntityData entityData);
        new TEntityData GetEntityData();
        new TKey GetKey(); 
    }

    public interface IDataDrivenEntityWithResult<TSelf, TKey, TEntityData, TInitializationResult>
        : IDataDrivenEntity<TSelf, TKey, TEntityData>, IInitializableWithResult
        where TInitializationResult : class
        where TEntityData : IEntityData<TKey>
        where TSelf : new()
        where TKey : notnull
    {
        TInitializationResult InitializeWithResult(TEntityData entityData);
    }
}
