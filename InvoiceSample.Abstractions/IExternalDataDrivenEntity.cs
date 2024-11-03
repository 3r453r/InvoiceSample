namespace InvoiceSample.DataDrivenEntity
{
    public interface IExternalDataDrivenEntity : IDataDrivenEntityBase
    {
        void Initialize(object entityData, object externalData, bool isNew = false);
    }

    public interface IDataDrivenEntity<TKey, TEntityData, TExternalData> : IExternalDataDrivenEntity
    where TEntityData : IEntityData<TKey>
    where TKey : notnull
    where TExternalData : class
    {
        void Initialize(TEntityData entityData, TExternalData externalData, bool isNew = false);
        new TEntityData GetEntityData();
    }
}
