using InvoiceSample.DataDrivenEntity.Aggregates;
using InvoiceSample.DataDrivenEntity.Implementations.Helpers;

namespace InvoiceSample.DataDrivenEntity.Implementations.Basic
{
    public abstract class DataDrivenEntity<TEntity, TKey, TEntityData>
        : IDataDrivenEntity<TEntity, TKey, TEntityData>, IAggregateEntity
        where TEntityData : IEntityData<TKey>
        where TEntity : new()
        where TKey : notnull
    {
        private List<ChildEntry> _childEntries = [];
        private List<ExternalChildEntry> _externalChildEntries = [];
        private List<CollectionEntry> _collectionEntries = [];
        private List<ExternalCollectionEntry> _externalCollectionEntries = [];

        private HashSet<IDataDrivenEntityBase> _allEntities = new();

        public bool IsInitialized { get; private set; }
        public bool IsNew { get; private set; } = true;
        protected abstract bool SelfInitialzed { get; }
        public IEnumerable<IDataDrivenEntityBase> GetAllEntities() => _allEntities;

        public abstract TEntityData GetEntityData();
        public abstract TKey GetKey();
        protected abstract void SelfInitialize(TEntityData entityData);

        object IDataDrivenEntityBase.GetEntityData() => GetEntityData();
        object IDataDrivenEntityBase.GetKey() => GetKey();

        public void Initialize(TEntityData entityData)
        {
            SelfInitialize(entityData);
            IsInitialized = InitializeAggregate(entityData) && SelfInitialzed;
            IsNew = false;
        }

        void IDataDrivenEntity.Initialize(object entityData)
        {
            if (entityData is TEntityData ed)
            {
                Initialize(ed);
            }
            else
            {
                throw new InvalidOperationException($"expecting data of type {typeof(TEntityData).Name}");
            }
        }

        public void RegisterChild<TChild, TChildKey, TChildData, TParentData, TParentKey>(
            TChild? child
            , Func<TParentData, TChildData?> childDataSelector
            , Action<IDataDrivenEntity> removeChild
            , Action<IDataDrivenEntity> setChild
            , Func<IDataDrivenEntity<TChild, TChildKey, TChildData>> childCreator)
            where TChild : IDataDrivenEntity<TChild, TChildKey, TChildData>, new()
            where TChildKey : notnull
            where TChildData : IEntityData<TChildKey>
            where TParentData : IEntityData<TParentKey>
            where TParentKey : notnull
        {
            _childEntries.Add(new ChildEntry
            {
                Entity = child,
                ChildDataSelector = parentData =>
                {
                    if (parentData is TParentData typedParentData)
                    {
                        return childDataSelector(typedParentData);
                    }
                    else
                    {
                        throw new InvalidCastException($"Expected parentData to be of type {typeof(TParentData)}, but got {parentData.GetType()}.");
                    }
                },
                RemoveChild = entity =>
                {
                    if (entity is IDataDrivenEntity<TChild, TChildKey, TChildData> typedEntity)
                    {
                        removeChild(typedEntity);
                    }
                    else
                    {
                        throw new InvalidCastException($"Expected entity to be of type {typeof(IDataDrivenEntity<TChild, TChildKey, TChildData>)}, but got {entity.GetType()}.");
                    }
                },
                ChildCreator = childCreator,
                SetChild = setChild,
            });
        }

        public void RegisterExternalChild<TChild, TChildKey, TChildData, TParentData, TParentKey, TExternalData>(
            TChild? child
            , Func<TParentData, TChildData?> childDataSelector
            , Action<IExternalDataDrivenEntity> removeChild
            , Action<IExternalDataDrivenEntity> setChild
            , Func<IDataDrivenEntity<TChild, TChildKey, TChildData, TExternalData>> childCreator
            , Func<TExternalData> externalDataProvider)
            where TChild : IDataDrivenEntity<TChild, TChildKey, TChildData, TExternalData>, new()
            where TChildKey : notnull
            where TChildData : IEntityData<TChildKey>
            where TParentData : IEntityData<TParentKey>
            where TParentKey : notnull
            where TExternalData : class
        {
            _externalChildEntries.Add(new ExternalChildEntry
            {
                Entity = child,
                ChildDataSelector = parentData =>
                {
                    if (parentData is TParentData typedParentData)
                    {
                        return childDataSelector(typedParentData);
                    }
                    else
                    {
                        throw new InvalidCastException($"Expected parentData to be of type {typeof(TParentData)}, but got {parentData.GetType()}.");
                    }
                },
                RemoveChild = entity =>
                {
                    if (entity is IDataDrivenEntity<TChild, TChildKey, TChildData, TExternalData> typedEntity)
                    {
                        removeChild(typedEntity);
                    }
                    else
                    {
                        throw new InvalidCastException($"Expected entity to be of type {typeof(IDataDrivenEntity<TChild, TChildKey, TChildData>)}, but got {entity.GetType()}.");
                    }
                },
                ChildCreator = childCreator,
                ExternalDataProvider = externalDataProvider,
                SetChild = setChild,
            });
        }

        public void RegisterChildCollection<TChild, TChildKey, TChildData, TParentData, TParentKey>
            (ICollection<TChild> collection
            , Func<TParentData, IEnumerable<TChildData>> childCollectionDataSelector
            , Func<IDataDrivenEntity<TChild, TChildKey, TChildData>> childCreator)
            where TChild : IDataDrivenEntity<TChild, TChildKey, TChildData>, new()
            where TChildKey : notnull
            where TChildData : IEntityData<TChildKey>
            where TParentData : IEntityData<TParentKey>
            where TParentKey : notnull
        {
            _collectionEntries.Add(new CollectionEntry
            {
                Collection = new CollectionWrapper<TChild>(collection),
                ChildCreator = childCreator,
                ChildCollectionDataSelector = parentData =>
                {
                    if (parentData is TParentData typedParentData)
                    {
                        return childCollectionDataSelector(typedParentData).Cast<IEntityData>();
                    }
                    else
                    {
                        throw new InvalidCastException($"Expected parentData to be of type {typeof(TParentData)}, but got {parentData.GetType()}.");
                    }
                },
            });
        }

        public void RegisterExternalChildCollection<TChild, TChildKey, TChildData, TParentData, TParentKey, TExternalData>(
            ICollection<TChild> collection
            , Func<TParentData, IEnumerable<TChildData>> childCollectionDataSelector
            , Func<IDataDrivenEntity<TChild, TChildKey, TChildData, TExternalData>> childCreator
            , Func<TExternalData> externalDataProvider)
            where TChild : IDataDrivenEntity<TChild, TChildKey, TChildData, TExternalData>, new()
            where TChildKey : notnull
            where TChildData : IEntityData<TChildKey>
            where TParentData : IEntityData<TParentKey>
            where TParentKey : notnull
            where TExternalData : class
        {
            _externalCollectionEntries.Add(new ExternalCollectionEntry
            {
                Collection = new ExternalCollectionWrapper<TChild>(collection),
                ChildCreator = childCreator,
                ChildCollectionDataSelector = parentData =>
                {
                    if (parentData is TParentData typedParentData)
                    {
                        return childCollectionDataSelector(typedParentData).Cast<IEntityData>();
                    }
                    else
                    {
                        throw new InvalidCastException($"Expected parentData to be of type {typeof(TParentData)}, but got {parentData.GetType()}.");
                    }
                },
                ExternalDataProvider = externalDataProvider
            });
        }

        private bool InitializeAggregate(TEntityData entityData)
        {
            var initialized = true;
            foreach (var childEntry in _childEntries) 
            { 
                var childData = childEntry.ChildDataSelector(entityData);
                if(childEntry.Entity is not null)
                {
                    if(childData is null)
                    {
                        childEntry.RemoveChild(childEntry.Entity);
                    }
                    else
                    {
                        childEntry.Entity.Initialize(childData);
                        AddEntities(childEntry.Entity);
                        initialized &= childEntry.Entity.IsInitialized;
                    }
                }
                else if (childData is not null)
                {
                    var newEntity = childEntry.ChildCreator();
                    newEntity.Initialize(childData);
                    AddEntities(newEntity);
                    childEntry.SetChild(newEntity);
                    initialized &= newEntity.IsInitialized;
                }
            }

            foreach (var childEntry in _externalChildEntries)
            {
                var childData = childEntry.ChildDataSelector(entityData);
                var externalData = childEntry.ExternalDataProvider();
                if (childEntry.Entity is not null)
                {
                    if(childData is null)
                    {
                        childEntry.RemoveChild(childEntry.Entity);
                    }
                    else
                    {
                        childEntry.Entity.Initialize(entityData, externalData);
                        AddEntities(childEntry.Entity);
                        initialized &= childEntry.Entity.IsInitialized;
                    }
                }
                else if(childData is not null)
                {
                    var newEntity = childEntry.ChildCreator();
                    newEntity.Initialize(childData, externalData);
                    childEntry.SetChild(newEntity);
                    AddEntities(newEntity);
                    initialized &= newEntity.IsInitialized;
                }
            }

            foreach(var collectionEntry in _collectionEntries)
            {
                var collectionData = collectionEntry.ChildCollectionDataSelector(entityData);
                var existingKeys = new List<object>();
                foreach(var childEntryData in collectionData)
                {
                    var key = childEntryData.GetKey();
                    existingKeys.Add(key);
                    var childEntry = collectionEntry.Collection.FirstOrDefault(e => e.GetKey().Equals(key));
                    if(childEntry is null)
                    {
                        childEntry = collectionEntry.ChildCreator();
                        collectionEntry.Collection.Add(childEntry);
                    }
                    childEntry.Initialize(childEntryData);
                    AddEntities(childEntry);
                    initialized &= childEntry.IsInitialized;
                }

                foreach(var entryToRemove in collectionEntry.Collection.Where(e => !existingKeys.Contains(e.GetKey())).ToArray())
                {
                    collectionEntry.Collection.Remove(entryToRemove);
                }
            }

            foreach (var collectionEntry in _externalCollectionEntries)
            {
                var collectionData = collectionEntry.ChildCollectionDataSelector(entityData);
                var externalData = collectionEntry.ExternalDataProvider();
                var existingKeys = new List<object>();
                foreach (var childEntryData in collectionData)
                {
                    var key = childEntryData.GetKey();
                    existingKeys.Add(key);
                    var childEntry = collectionEntry.Collection.FirstOrDefault(e => e.GetKey().Equals(key));
                    if (childEntry is null)
                    {
                        childEntry = collectionEntry.ChildCreator();
                        collectionEntry.Collection.Add(childEntry);
                    }
                    childEntry.Initialize(childEntryData, externalData);
                    AddEntities(childEntry);
                    initialized &= childEntry.IsInitialized;
                }

                foreach (var entryToRemove in collectionEntry.Collection.Where(e => !existingKeys.Contains(e.GetKey())).ToArray())
                {
                    collectionEntry.Collection.Remove(entryToRemove);
                }
            }

            return initialized;
        }

        private void AddEntities(IDataDrivenEntityBase entity)
        {
            _allEntities.Add(entity);
            foreach (var entit in entity.GetAllEntities())
            {
                _allEntities.Add(entity);
            }
        }
    }
}
