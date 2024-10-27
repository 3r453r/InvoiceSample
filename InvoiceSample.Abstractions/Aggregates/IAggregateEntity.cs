namespace InvoiceSample.DataDrivenEntity.Aggregates
{
    public interface IAggregateEntity
    {
        void RegisterChild<TChild, TChildKey, TChildData, TParentData, TParentKey>(
             TChild? child
            , Func<TParentData, TChildData?> childDataSelector
            , Action<IDataDrivenEntity> removeChild
            , Action<IDataDrivenEntity> setChild
            , Func<IDataDrivenEntity<TChild, TChildKey, TChildData>> childCreator
            )
            where TChild : IDataDrivenEntity<TChild, TChildKey, TChildData>, new()
            where TChildKey : notnull
            where TChildData : IEntityData<TChildKey>
            where TParentData : IEntityData<TParentKey>
            where TParentKey : notnull
            ;

        void RegisterExternalChild<TChild, TChildKey, TChildData, TParentData, TParentKey, TExternalData>(
             TChild? child
            , Func<TParentData, TChildData?> childDataSelector
            , Action<IExternalDataDrivenEntity> removeChild
            , Action<IExternalDataDrivenEntity> setChild
            , Func<IDataDrivenEntity<TChild, TChildKey, TChildData, TExternalData>> childCreator
            , Func<TExternalData> externalDataProvider
            )
            where TChild : IDataDrivenEntity<TChild, TChildKey, TChildData, TExternalData>, new()
            where TChildKey : notnull
            where TChildData : IEntityData<TChildKey>
            where TParentData : IEntityData<TParentKey>
            where TParentKey : notnull
            where TExternalData : class
            ;

        void RegisterChildCollection<TChild, TChildKey, TChildData, TParentData, TParentKey>(
            ICollection<TChild> collection
            , Func<TParentData, IEnumerable<TChildData>> childCollectionDataSelector
            , Func<IDataDrivenEntity<TChild, TChildKey, TChildData>> childCreator
            )
            where TChild : IDataDrivenEntity<TChild, TChildKey, TChildData>, new()
            where TChildKey : notnull
            where TChildData : IEntityData<TChildKey>
            where TParentData : IEntityData<TParentKey>
            where TParentKey : notnull
            ;

        void RegisterExternalChildCollection<TChild, TChildKey, TChildData, TParentData, TParentKey, TExternalData>(
            ICollection<TChild> collection
            , Func<TParentData, IEnumerable<TChildData>> childCollectionDataSelector
            , Func<IDataDrivenEntity<TChild, TChildKey, TChildData, TExternalData>> childCreator
            , Func<TExternalData> externalDataProvider
            )
            where TChild : IDataDrivenEntity<TChild, TChildKey, TChildData, TExternalData>, new()
            where TChildKey : notnull
            where TChildData : IEntityData<TChildKey>
            where TParentData : IEntityData<TParentKey>
            where TParentKey : notnull
            where TExternalData : class
            ;
    }
}
