using InvoiceSample.DataDrivenEntity.Aggregates;
using InvoiceSample.DataDrivenEntity.Implementations.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceSample.DataDrivenEntity.Implementations.Basic
{
    public abstract class DataDrivenEntity<TKey, TEntityData>
        : DataDrivenEntityBase<TKey>, IDataDrivenEntity<TKey, TEntityData>, IAggregateEntity<TKey, TEntityData>
        where TEntityData : IEntityData<TKey>
        where TKey : notnull
    {
        // These methods can remain as they are in derived classes
        protected abstract void SelfInitialize(TEntityData entityData);

        public abstract TEntityData GetEntityData();
        public abstract TKey GetKey();
        // We override the Core methods from base class
        // These provide the implementation for the interface methods
        protected override object GetEntityDataCore() => GetEntityData();
        protected override object GetKeyCore() => GetKey();

        public void Initialize(TEntityData entityData, IInitializationContext? context = null, bool isNew = false)
        {
            IsNew = isNew;

            // Self initialization first
            SelfInitialize(entityData);

            _initializationContext = context ?? new InitializationContext();

            // Check if already initialized in this context
            if (_initializationContext.IsInitialized(this))
            {
                return;
            }


            // Add the entity to our own collection
            _allEntities.Add(this);

            // Register with the context before initializing children
            _initializationContext.Add(this);

            // Subscribe to initialization events for relationship resolution
            if (_relationshipResolvers.Count > 0)
            {
                _initializationContext.SubscribeToInitializationEvents(ResolveRelationships);
            }

            // Initialize aggregate relationships
            IsInitialized = InitializeAggregate(entityData, isNew, _initializationContext) && SelfInitialzed;
        }

        void IDataDrivenEntity.Initialize(object entityData, IInitializationContext? initializationContext, bool isNew)
        {
            if (entityData is TEntityData ed)
            {
                Initialize(ed, initializationContext, isNew);
            }
            else
            {
                throw new InvalidOperationException($"expecting data of type {typeof(TEntityData).Name}");
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

            // Add a relationship resolver for this child
            RegisterRelationshipResolver(evt => {
                if (child != null && evt.Entity is IDataDrivenEntity<TChildKey, TChildData> entityToCheck &&
                    Object.Equals(child.GetKey(), entityToCheck.GetKey()))
                {
                    // Update our reference only if the entity is different
                    if (!ReferenceEquals(child, entityToCheck))
                    {
                        setChild((IDataDrivenEntity)evt.Entity);
                    }
                }
            });
        }

        public void RegisterExternalChild<TChildKey, TChildData, TExternalData>(
            IDataDrivenEntity<TChildKey, TChildData, TExternalData>? child
            , Func<TEntityData, TChildData?> childDataSelector
            , Action<IExternalDataDrivenEntity> removeChild
            , Action<IExternalDataDrivenEntity> setChild
            , Func<TEntityData, IDataDrivenEntity<TChildKey, TChildData, TExternalData>> childCreator
            , Func<TEntityData, TExternalData> externalDataProvider)
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

            // Add a relationship resolver for this external child
            RegisterRelationshipResolver(evt => {
                if (child != null && evt.Entity is IDataDrivenEntity<TChildKey, TChildData, TExternalData> entityToCheck &&
                    Object.Equals(child.GetKey(), entityToCheck.GetKey()))
                {
                    // Update our reference only if the entity is different
                    if (!ReferenceEquals(child, entityToCheck))
                    {
                        setChild((IExternalDataDrivenEntity)evt.Entity);
                    }
                }
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

            // Add a relationship resolver for collection items
            RegisterRelationshipResolver(evt => {
                if (evt.Entity is TChild childEntity)
                {
                    bool alreadyContains = collection.Any(e => Object.Equals(e.GetKey(), childEntity.GetKey()));

                    // If it's already in the collection and it's the same instance, do nothing
                    if (alreadyContains && collection.Contains(childEntity))
                        return;

                    // If it's already in the collection but a different instance with the same key, replace it
                    if (alreadyContains)
                    {
                        var existingEntity = collection.First(e => Object.Equals(e.GetKey(), childEntity.GetKey()));
                        if (!ReferenceEquals(existingEntity, childEntity))
                        {
                            collection.Remove(existingEntity);
                            collection.Add(childEntity);
                            AddEntities(childEntity);
                        }
                    }
                    // If it's not in the collection, but should be based on its data, add it
                    else if (childEntity.IsInitialized)
                    {
                        // We need a reference to the current entity data to determine if this entity belongs in the collection
                        var currentData = GetEntityData();
                        if (currentData != null)
                        {
                            // Check if the new entity belongs in this collection based on its data
                            // This is a simplified check - in a real implementation you might need more logic
                            var childDatas = childCollectionDataSelector(currentData);
                            var belongsInCollection = childDatas.Any(cd => Object.Equals(cd.GetKey(), childEntity.GetKey()));

                            if (belongsInCollection && !collection.Contains(childEntity))
                            {
                                collection.Add(childEntity);
                                AddEntities(childEntity);
                            }
                        }
                    }
                }
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

            // Add a relationship resolver for external collection items
            RegisterRelationshipResolver(evt => {
                if (evt.Entity is TChild childEntity)
                {
                    bool alreadyContains = collection.Any(e => Object.Equals(e.GetKey(), childEntity.GetKey()));

                    // If it's already in the collection and it's the same instance, do nothing
                    if (alreadyContains && collection.Contains(childEntity))
                        return;

                    // If it's already in the collection but a different instance with the same key, replace it
                    if (alreadyContains)
                    {
                        var existingEntity = collection.First(e => Object.Equals(e.GetKey(), childEntity.GetKey()));
                        if (!ReferenceEquals(existingEntity, childEntity))
                        {
                            collection.Remove(existingEntity);
                            collection.Add(childEntity);
                            AddEntities(childEntity);
                        }
                    }
                    // If it's not in the collection at all, we'll let normal initialization handle it
                }
            });
        }

        private bool InitializeAggregate(TEntityData entityData, bool isNew, IInitializationContext context)
        {
            return InitializeAggregateBase(entityData, isNew, context);
        }
    }
}