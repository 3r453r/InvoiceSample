using InvoiceSample.DataDrivenEntity.Aggregates;
using InvoiceSample.DataDrivenEntity.Implementations.Helpers;

namespace InvoiceSample.DataDrivenEntity.Implementations.Basic
{
    public abstract class DataDrivenEntity<TKey, TEntityData>
        : IDataDrivenEntity<TKey, TEntityData>, IAggregateEntity<TKey, TEntityData>
        where TEntityData : IEntityData<TKey>
        where TKey : notnull
    {
        private List<ChildEntry> _childEntries = [];
        private List<ExternalChildEntry> _externalChildEntries = [];
        private List<CollectionEntry> _collectionEntries = [];
        private List<ExternalCollectionEntry> _externalCollectionEntries = [];

        private HashSet<IDataDrivenEntityBase> _allEntities = new();

        public bool IsInitialized { get; private set; }
        public bool IsNew { get; set; }
        protected abstract bool SelfInitialzed { get; }
        public IEnumerable<IDataDrivenEntityBase> GetAllEntities() => _allEntities;

        public abstract TEntityData GetEntityData();
        public abstract TKey GetKey();
        protected abstract void SelfInitialize(TEntityData entityData);

        object IDataDrivenEntityBase.GetEntityData() => GetEntityData();
        object IDataDrivenEntityBase.GetKey() => GetKey();

        public void Initialize(TEntityData entityData, bool isNew = false)
        {
            SelfInitialize(entityData);
            IsInitialized = InitializeAggregate(entityData, isNew) && SelfInitialzed;
            IsNew = isNew;
        }

        void IDataDrivenEntity.Initialize(object entityData, bool isNew)
        {
            if (entityData is TEntityData ed)
            {
                Initialize(ed, isNew);
            }
            else
            {
                throw new InvalidOperationException($"expecting data of type {typeof(TEntityData).Name}");
            }
        }

        public void RegisterChild<TChild, TChildKey, TChildData>(
            TChild? child
            , Func<TEntityData, TChildData?> childDataSelector
            , Action<IDataDrivenEntity> removeChild
            , Action<IDataDrivenEntity> setChild
            , Func<TEntityData, TChild> childCreator)
            where TChild : IDataDrivenEntity<TChildKey, TChildData>
            where TChildKey : notnull
            where TChildData : IEntityData<TChildKey>

        {
            _childEntries.Add(new ChildEntry
            {
                Entity = child,
                ChildDataSelector = parentData =>
                {
                    if (parentData is TEntityData typedParentData)
                    {
                        return childDataSelector(typedParentData);
                    }
                    else
                    {
                        throw new InvalidCastException($"Expected parentData to be of type {typeof(TEntityData)}, but got {parentData.GetType()}.");
                    }
                },
                RemoveChild = entity =>
                {
                    if (entity is IDataDrivenEntity<TChildKey, TChildData> typedEntity)
                    {
                        removeChild(typedEntity);
                    }
                    else
                    {
                        throw new InvalidCastException($"Expected entity to be of type {typeof(IDataDrivenEntity<TChildKey, TChildData>)}, but got {entity.GetType()}.");
                    }
                },
                ChildCreator = (p) => childCreator((TEntityData)p),
                SetChild = setChild,
            });
        }

        public void RegisterExternalChild<TChild, TChildKey, TChildData, TExternalData>(
            TChild? child
            , Func<TEntityData, TChildData?> childDataSelector
            , Action<IExternalDataDrivenEntity> removeChild
            , Action<IExternalDataDrivenEntity> setChild
            , Func<TEntityData, TChild> childCreator
            , Func<TEntityData, TExternalData> externalDataProvider)
            where TChild : IDataDrivenEntity<TChildKey, TChildData, TExternalData>
            where TChildKey : notnull
            where TChildData : IEntityData<TChildKey>
            where TExternalData : class
        {
            _externalChildEntries.Add(new ExternalChildEntry
            {
                Entity = child,
                ChildDataSelector = parentData =>
                {
                    if (parentData is TEntityData typedParentData)
                    {
                        return childDataSelector(typedParentData);
                    }
                    else
                    {
                        throw new InvalidCastException($"Expected parentData to be of type {typeof(TEntityData)}, but got {parentData.GetType()}.");
                    }
                },
                RemoveChild = entity =>
                {
                    if (entity is IDataDrivenEntity<TChildKey, TChildData, TExternalData> typedEntity)
                    {
                        removeChild(typedEntity);
                    }
                    else
                    {
                        throw new InvalidCastException($"Expected entity to be of type {typeof(IDataDrivenEntity<TChildKey, TChildData>)}, but got {entity.GetType()}.");
                    }
                },
                ChildCreator = (p) => childCreator((TEntityData)p)
                ,
                ExternalDataProvider = parentData => 
                { 
                    if(parentData is TEntityData typedParentData)
                    {
                        return externalDataProvider(typedParentData);
                    }
                    else
                    {
                        throw new InvalidCastException($"Expected parentData to be of type {typeof(TEntityData)}, but got {parentData.GetType()}.");
                    }
                },
                SetChild = setChild,
            });
        }

        public void RegisterChildCollection<TChild, TChildKey, TChildData>
            (ICollection<TChild> collection
            , Func<TEntityData, IEnumerable<TChildData>> childCollectionDataSelector
            , Func<TEntityData, TChildData, TChild> childCreator)
            where TChild : IDataDrivenEntity<TChildKey, TChildData>
            where TChildKey : notnull
            where TChildData : IEntityData<TChildKey>
        {
            _collectionEntries.Add(new CollectionEntry
            {
                Collection = new CollectionWrapper<TChild>(collection),
                ChildCreator = (p, c) => childCreator((TEntityData)p, (TChildData)c),
                ChildCollectionDataSelector = parentData =>
                {
                    if (parentData is TEntityData typedParentData)
                    {
                        return childCollectionDataSelector(typedParentData).Cast<IEntityData>();
                    }
                    else
                    {
                        throw new InvalidCastException($"Expected parentData to be of type {typeof(TEntityData)}, but got {parentData.GetType()}.");
                    }
                },
            });
        }

        public void RegisterExternalChildCollection<TChild, TChildKey, TChildData, TExternalData>(
            ICollection<TChild> collection
            , Func<TEntityData, IEnumerable<TChildData>> childCollectionDataSelector
            , Func<TEntityData, TChildData, TChild> childCreator
            , Func<TEntityData, TExternalData> externalDataProvider)
            where TChild : IDataDrivenEntity<TChildKey, TChildData, TExternalData>
            where TChildKey : notnull
            where TChildData : IEntityData<TChildKey>
            where TExternalData : class
        {
            _externalCollectionEntries.Add(new ExternalCollectionEntry
            {
                Collection = new ExternalCollectionWrapper<TChild>(collection),
                ChildCreator = (p, c) => childCreator((TEntityData)p, (TChildData)c),
                ChildCollectionDataSelector = parentData =>
                {
                    if (parentData is TEntityData typedParentData)
                    {
                        return childCollectionDataSelector(typedParentData).Cast<IEntityData>();
                    }
                    else
                    {
                        throw new InvalidCastException($"Expected parentData to be of type {typeof(TEntityData)}, but got {parentData.GetType()}.");
                    }
                },
                ExternalDataProvider = parentData =>
                {
                    if (parentData is TEntityData typedParentData)
                    {
                        return externalDataProvider(typedParentData);
                    }
                    else
                    {
                        throw new InvalidCastException($"Expected parentData to be of type {typeof(TEntityData)}, but got {parentData.GetType()}.");
                    }
                }
            });
        }

        private bool InitializeAggregate(TEntityData entityData, bool isNew)
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
                        childEntry.Entity.Initialize(childData, isNew);
                        AddEntities(childEntry.Entity);
                        initialized &= childEntry.Entity.IsInitialized;
                    }
                }
                else if (childData is not null)
                {
                    var newEntity = childEntry.ChildCreator(entityData);
                    newEntity.Initialize(childData, isNew);
                    AddEntities(newEntity);
                    childEntry.Entity = newEntity;
                    childEntry.SetChild(newEntity);
                    initialized &= newEntity.IsInitialized;
                }
            }

            foreach (var childEntry in _externalChildEntries)
            {
                var childData = childEntry.ChildDataSelector(entityData);
                var externalData = childEntry.ExternalDataProvider(entityData);
                if (childEntry.Entity is not null)
                {
                    if(childData is null)
                    {
                        childEntry.RemoveChild(childEntry.Entity);
                    }
                    else
                    {
                        childEntry.Entity.Initialize(entityData, externalData, isNew);
                        AddEntities(childEntry.Entity);
                        initialized &= childEntry.Entity.IsInitialized;
                    }
                }
                else if(childData is not null)
                {
                    var newEntity = childEntry.ChildCreator(entityData);
                    newEntity.Initialize(childData, externalData, isNew);
                    childEntry.Entity = newEntity;
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
                        childEntry = collectionEntry.ChildCreator(entityData, childEntryData);
                        collectionEntry.Collection.Add(childEntry);
                    }
                    childEntry.Initialize(childEntryData, isNew);
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
                var externalData = collectionEntry.ExternalDataProvider(entityData);
                var existingKeys = new List<object>();
                foreach (var childEntryData in collectionData)
                {
                    var key = childEntryData.GetKey();
                    existingKeys.Add(key);
                    var childEntry = collectionEntry.Collection.FirstOrDefault(e => e.GetKey().Equals(key));
                    if (childEntry is null)
                    {
                        childEntry = collectionEntry.ChildCreator(entityData, childEntryData);
                        collectionEntry.Collection.Add(childEntry);
                    }
                    childEntry.Initialize(childEntryData, externalData, isNew);
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
            foreach (var child in entity.GetAllEntities())
            {
                _allEntities.Add(child);
            }
        }
    }
}
