namespace InvoiceSample.DataDrivenEntity
{
    public interface IDataDrivenEntity
    {
        void Initialize(object entityData);
        object GetEntityData();
        object GetKey();
        bool Initialized();
    }

    public interface IDataDrivenEntity<TEntity, TKey, TEntityData> : IDataDrivenEntity
        where TEntityData : IEntityData<TKey>
        where TEntity : new()
        where TKey : notnull
    {
        void Initialize(TEntityData entityData);
        new TEntityData GetEntityData();
        new TKey GetKey(); 
    }

    public interface IDataDrivenEntityWithResult<TEntity, TKey, TInitializationResult, TEntityData>
        : IDataDrivenEntity<TEntity, TKey, TEntityData>
        where TInitializationResult : class
        where TEntityData : IEntityData<TKey>
        where TEntity : new()
        where TKey : notnull
    {
        TInitializationResult InitializeWithResult(TEntityData entityData);
    }

    public interface IExternalDataDrivenEntity
    {
        void Initialize(object entityData, object externalData);
        object GetEntityData();
        object GetKey();
        bool Initialized();
    }

    public interface IDataDrivenEntity<TEntity, TKey, TEntityData, TExternalData> : IExternalDataDrivenEntity
        where TEntityData : IEntityData<TKey>
        where TEntity : new()
        where TKey : notnull
        where TExternalData : class
    {
        void Initialize(TEntityData entityData, TExternalData externalData);
        TEntityData GetEntityData();
        bool Initialized();
    }

    public interface IDataDrivenEntityWithResult<TEntity, TKey, TInitializationResult, TEntityData, TExternalData>
        : IDataDrivenEntity<TEntity, TKey, TEntityData, TExternalData>
        where TInitializationResult : class
        where TEntityData : IEntityData<TKey>
        where TEntity : new()
        where TKey : notnull
    where TExternalData : class
    {
        TInitializationResult InitializeWithResult(TEntityData entityData, TExternalData externalData);
    }
}
