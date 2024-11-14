namespace InvoiceSample.DataDrivenEntity
{
    public interface IExternalDataDrivenEntity : IDataDrivenEntityBase
    {
        void Initialize(object entityData, object externalData, IInitializationContext? initializationContext, bool isNew = false);
    }

    public interface IDataDrivenEntity<TKey, TEntityData, TExternalData> : IExternalDataDrivenEntity
    where TEntityData : IEntityData<TKey>
    where TKey : notnull
    where TExternalData : class
    {
        void Initialize(TEntityData entityData, TExternalData externalData, IInitializationContext? context = null, bool isNew = false);
        new TEntityData GetEntityData();
    }
}
