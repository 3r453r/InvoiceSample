using InvoiceSample.DataDrivenEntity.Aggregates;
using InvoiceSample.DataDrivenEntity.Implementations.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceSample.DataDrivenEntity.Implementations
{
    public abstract class ExternalDataDrivenEntity<TKey, TEntityData, TExternalData>
        : DataDrivenEntityBase<TKey>, IDataDrivenEntity<TKey, TEntityData, TExternalData>, IAggregateEntity<TKey, TEntityData>
        where TEntityData : IEntityData<TKey>
        where TKey : notnull
        where TExternalData : class
    {
        // These methods can remain as they are in derived classes
        public abstract TEntityData GetEntityData();
        public abstract TKey GetKey();
        protected abstract void SelfInitialize(TEntityData entityData, TExternalData externalData);

        // Implement IDataDrivenEntityBase methods
        protected override object GetEntityDataCore() => GetEntityData();
        protected override object GetKeyCore() => GetKey();

        public void Initialize(TEntityData entityData, TExternalData externalData, IInitializationContext? context = null, bool isNew = false)
        {
            // Set IsNew flag
            IsNew = isNew;

            SelfInitialize(entityData, externalData);
            _initializationContext = context is null ? new InitializationContext() : context;
            if (_initializationContext.IsInitialized(this))
            {
                return;
            }

            // Add ourselves to our own collection
            _allEntities.Add(this);

            // Register with the context
            _initializationContext.Add(this);

            // Subscribe to initialization events if we have relationship resolvers
            if (_relationshipResolvers.Count > 0)
            {
                _initializationContext.SubscribeToInitializationEvents(ResolveRelationships);
            }

            IsInitialized = InitializeAggregate(entityData, isNew, _initializationContext) && SelfInitialzed;
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

            // Register relationship resolver for this child
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

            // Register relationship resolver for this external child
            RegisterRelationshipResolver(evt => {
                if (child != null && evt.Entity is IDataDrivenEntity<TChildKey, TChildData, TChildExternalData> entityToCheck &&
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

            // Register relationship resolver for this collection
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
                }
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

            // Register relationship resolver for this external collection
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
                }
            });
        }

        private bool InitializeAggregate(TEntityData entityData, bool isNew, IInitializationContext context)
        {
            return InitializeAggregateBase(entityData, isNew, context);
        }
    }
}