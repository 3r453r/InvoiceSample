﻿namespace InvoiceSample.DataDrivenEntity.Aggregates
{
    public interface IAggregateEntity
    {
        void RegisterChild<TChild, TChildKey, TChildData, TParentData, TParentKey>(
             TChild? child
            , Func<TParentData, TChildData?> childDataSelector
            , Action<IDataDrivenEntity> removeChild
            , Action<IDataDrivenEntity> setChild
            , Func<TParentData, TChild> childCreator
            )
            where TChild : IDataDrivenEntity<TChildKey, TChildData>
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
            , Func<TParentData, TChild> childCreator
            , Func<TParentData, TExternalData> externalDataProvider
            )
            where TChild : IDataDrivenEntity<TChildKey, TChildData, TExternalData>
            where TChildKey : notnull
            where TChildData : IEntityData<TChildKey>
            where TParentData : IEntityData<TParentKey>
            where TParentKey : notnull
            where TExternalData : class
            ;

        void RegisterChildCollection<TChild, TChildKey, TChildData, TParentData, TParentKey>(
            ICollection<TChild> collection
            , Func<TParentData, IEnumerable<TChildData>> childCollectionDataSelector
            , Func<TParentData, TChildData, TChild> childCreator
            )
            where TChild : IDataDrivenEntity<TChildKey, TChildData>
            where TChildKey : notnull
            where TChildData : IEntityData<TChildKey>
            where TParentData : IEntityData<TParentKey>
            where TParentKey : notnull
            ;

        void RegisterExternalChildCollection<TChild, TChildKey, TChildData, TParentData, TParentKey, TExternalData>(
            ICollection<TChild> collection
            , Func<TParentData, IEnumerable<TChildData>> childCollectionDataSelector
            , Func<TParentData, TChildData, TChild> childCreator
            , Func<TParentData, TExternalData> externalDataProvider
            )
            where TChild : IDataDrivenEntity<TChildKey, TChildData, TExternalData>
            where TChildKey : notnull
            where TChildData : IEntityData<TChildKey>
            where TParentData : IEntityData<TParentKey>
            where TParentKey : notnull
            where TExternalData : class
            ;
    }

    public interface IAggregateEntity<TParentKey, TParentData>
        where TParentData : IEntityData<TParentKey>
        where TParentKey : notnull
    {
        void RegisterChild<TChild, TChildKey, TChildData>(
             TChild? child
            , Func<TParentData, TChildData?> childDataSelector
            , Action<IDataDrivenEntity> removeChild
            , Action<IDataDrivenEntity> setChild
            , Func<TParentData, TChild> childCreator
            )
            where TChild : IDataDrivenEntity<TChildKey, TChildData>
            where TChildKey : notnull
            where TChildData : IEntityData<TChildKey>

            ;

        void RegisterExternalChild<TChild, TChildKey, TChildData, TExternalData>(
             TChild? child
            , Func<TParentData, TChildData?> childDataSelector
            , Action<IExternalDataDrivenEntity> removeChild
            , Action<IExternalDataDrivenEntity> setChild
            , Func<TParentData, TChild> childCreator
            , Func<TParentData, TExternalData> externalDataProvider
            )
            where TChild : IDataDrivenEntity<TChildKey, TChildData, TExternalData>
            where TChildKey : notnull
            where TChildData : IEntityData<TChildKey>
            where TExternalData : class
            ;

        void RegisterChildCollection<TChild, TChildKey, TChildData>(
            ICollection<TChild> collection
            , Func<TParentData, IEnumerable<TChildData>> childCollectionDataSelector
            , Func<TParentData, TChildData, TChild> childCreator
            )
            where TChild : IDataDrivenEntity<TChildKey, TChildData>
            where TChildKey : notnull
            where TChildData : IEntityData<TChildKey>
            ;

        void RegisterExternalChildCollection<TChild, TChildKey, TChildData, TExternalData>(
            ICollection<TChild> collection
            , Func<TParentData, IEnumerable<TChildData>> childCollectionDataSelector
            , Func<TParentData, TChildData, TChild> childCreator
            , Func<TParentData, TExternalData> externalDataProvider
            )
            where TChild : IDataDrivenEntity<TChildKey, TChildData, TExternalData>
            where TChildKey : notnull
            where TChildData : IEntityData<TChildKey>
            where TExternalData : class
            ;
    }
}
