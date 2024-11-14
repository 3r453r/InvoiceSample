using InvoiceSample.DataDrivenEntity.Aggregates;
using InvoiceSample.DataDrivenEntity.Implementations.Helpers;

namespace InvoiceSample.DataDrivenEntity.Implementations
{
    public abstract class ExternalDataDrivenEntity<TKey, TEntityData, TExternalData>
        : IDataDrivenEntity<TKey, TEntityData, TExternalData>, IAggregateEntity<TKey, TEntityData>
        where TEntityData : IEntityData<TKey>
        where TKey : notnull
        where TExternalData : class
    {
        private List<ChildEntry> _childEntries = [];
        private List<ExternalChildEntry> _externalChildEntries = [];
        private List<CollectionEntry> _collectionEntries = [];
        private List<ExternalCollectionEntry> _externalCollectionEntries = [];
        private HashSet<IDataDrivenEntityBase> _allEntities = new();
        private IInitializationContext? _initializationContext;

        public bool IsInitialized { get; private set; }
        public bool IsNew { get; set; } = true;
        protected abstract bool SelfInitialzed { get; }
        public IEnumerable<IDataDrivenEntityBase> GetAllEntities() => _allEntities;

        public abstract TEntityData GetEntityData();
        object IDataDrivenEntityBase.GetEntityData() => GetEntityData();

        public abstract TKey GetKey();
        protected abstract void SelfInitialize(TEntityData entityData, TExternalData externalData);

        public void Initialize(TEntityData entityData, TExternalData externalData, IInitializationContext? context = null, bool isNew = false)
        {
            SelfInitialize(entityData, externalData);

            _initializationContext = context is null ? new InitializationContext() : context;
            if (_initializationContext.IsInitialized(this))
            {
                return;
            }

            _initializationContext.Add(this);

            IsInitialized = InitializeAggregate(entityData, isNew, _initializationContext) && SelfInitialzed;
            IsNew = isNew;
        }

        void IExternalDataDrivenEntity.Initialize(object entityData, object externalData, IInitializationContext? initializationContext, bool isNew)
        {
            if (entityData is TEntityData enD && externalData is TExternalData exD)
            {
                Initialize(enD, exD, initializationContext, isNew);
            }
            else
            {
                throw new InvalidCastException($"expecting types {typeof(TEntityData).Name} and {typeof(TExternalData).Name}");
            }
        }

        object IDataDrivenEntityBase.GetKey() => GetKey();

        private bool InitializeAggregate(TEntityData entityData, bool isNew, IInitializationContext context)
        {
            var initialized = true;
            foreach (var childEntry in _childEntries)
            {
                var childData = childEntry.ChildDataSelector(entityData);
                if (childEntry.Entity is not null)
                {
                    if (childData is null)
                    {
                        childEntry.RemoveChild(childEntry.Entity);
                    }
                    else
                    {
                        childEntry.Entity.Initialize(childData, context, isNew);
                        AddEntities(childEntry.Entity);
                        initialized &= childEntry.Entity.IsInitialized;
                    }
                }
                else if (childData is not null)
                {
                    var newEntity = childEntry.ChildCreator(entityData);
                    newEntity.Initialize(childData, context, isNew);
                    AddEntities(newEntity);
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
                    if (childData is null)
                    {
                        childEntry.RemoveChild(childEntry.Entity);
                    }
                    else
                    {
                        childEntry.Entity.Initialize(entityData, externalData, context, isNew);
                        AddEntities(childEntry.Entity);
                        initialized &= childEntry.Entity.IsInitialized;
                    }
                }
                else if (childData is not null)
                {
                    var newEntity = childEntry.ChildCreator(entityData);
                    newEntity.Initialize(childData, externalData, context, isNew);
                    childEntry.SetChild(newEntity);
                    AddEntities(newEntity);
                    initialized &= newEntity.IsInitialized;
                }
            }

            foreach (var collectionEntry in _collectionEntries)
            {
                var collectionData = collectionEntry.ChildCollectionDataSelector(entityData);
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
                    childEntry.Initialize(childEntryData, context, isNew);
                    AddEntities(childEntry);
                    initialized &= childEntry.IsInitialized;
                }

                foreach (var entryToRemove in collectionEntry.Collection.Where(e => !existingKeys.Contains(e.GetKey())).ToArray())
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
                    childEntry.Initialize(childEntryData, externalData, context, isNew);
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

        public void RegisterChild<TChildKey, TChildData>(
            IDataDrivenEntity<TChildKey, TChildData>? child
            , Func<TEntityData, TChildData?> childDataSelector
            , Action<IDataDrivenEntity> removeChild
            , Action<IDataDrivenEntity> setChild
            , Func<TEntityData, IDataDrivenEntity<TChildKey, TChildData>> childCreator)
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

        public void RegisterExternalChild<TChildKey, TChildData, TChildExternalData>(
            IDataDrivenEntity<TChildKey, TChildData, TChildExternalData>? child
            , Func<TEntityData, TChildData?> childDataSelector
            , Action<IExternalDataDrivenEntity> removeChild
            , Action<IExternalDataDrivenEntity> setChild
            , Func<TEntityData, IDataDrivenEntity<TChildKey, TChildData, TChildExternalData>> childCreator
            , Func<TEntityData, TChildExternalData> externalDataProvider)
            where TChildKey : notnull
            where TChildData : IEntityData<TChildKey>
            where TChildExternalData : class
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
                    if (entity is IDataDrivenEntity<TChildKey, TChildData, TChildExternalData> typedEntity)
                    {
                        removeChild(typedEntity);
                    }
                    else
                    {
                        throw new InvalidCastException($"Expected entity to be of type {typeof(IDataDrivenEntity<TChildKey, TChildData>)}, but got {entity.GetType()}.");
                    }
                },
                ChildCreator = (p) => childCreator((TEntityData)p),
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
                },
                SetChild = setChild,
            });
        }

        public void RegisterChildCollection<TChild, TChildKey, TChildData>(
            ICollection<TChild> collection
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

        public void RegisterExternalChildCollection<TChild, TChildKey, TChildData, TChildExternalData>(
            ICollection<TChild> collection
            , Func<TEntityData, IEnumerable<TChildData>> childCollectionDataSelector
            , Func<TEntityData, TChildData, TChild> childCreator
            , Func<TEntityData, TChildExternalData> externalDataProvider)
            where TChild : IDataDrivenEntity<TChildKey, TChildData, TChildExternalData>
            where TChildKey : notnull
            where TChildData : IEntityData<TChildKey>
            where TChildExternalData : class
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
    }
}
