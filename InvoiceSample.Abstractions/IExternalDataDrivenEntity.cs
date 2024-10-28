namespace InvoiceSample.DataDrivenEntity
{
    public interface IExternalDataDrivenEntity : IDataDrivenEntityBase
    {
        object? GetExternalData(object entityData);
        void Initialize(object entityData, object externalData);
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
}
