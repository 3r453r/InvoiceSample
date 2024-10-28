namespace InvoiceSample.DataDrivenEntity
{
    public interface IDataDrivenEntity : IDataDrivenEntityBase
    {
        void Initialize(object entityData);
    }

    public interface IDataDrivenEntity<TSelf, TKey, TEntityData> 
        : IDataDrivenEntity
        where TSelf : new()
        where TEntityData : IEntityData<TKey>
        where TKey : notnull
    {
        void Initialize(TEntityData entityData);
        new TEntityData GetEntityData();
        new TKey GetKey(); 
    }
}
