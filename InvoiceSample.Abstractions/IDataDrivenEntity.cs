namespace InvoiceSample.DataDrivenEntity
{
    public interface IDataDrivenEntity : IDataDrivenEntityBase
    {
        void Initialize(object entityData, bool isNew = false);
    }

    public interface IDataDrivenEntity<TKey, TEntityData> 
        : IDataDrivenEntity
        where TEntityData : IEntityData<TKey>
        where TKey : notnull
    {
        void Initialize(TEntityData entityData, bool isNew = false);
        new TEntityData GetEntityData();
        new TKey GetKey(); 
    }
}
