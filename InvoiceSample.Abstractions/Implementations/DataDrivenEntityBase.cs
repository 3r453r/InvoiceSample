using InvoiceSample.DataDrivenEntity.Aggregates;
using InvoiceSample.DataDrivenEntity.Implementations.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InvoiceSample.DataDrivenEntity.Implementations
{
    /// <summary>
    /// Base class for all data-driven entities that provides common functionality
    /// </summary>
    public abstract class DataDrivenEntityBase<TKey> : IDataDrivenEntityBase
        where TKey : notnull
    {
        protected List<ChildEntry> _childEntries = [];
        protected List<ExternalChildEntry> _externalChildEntries = [];
        protected List<CollectionEntry> _collectionEntries = [];
        protected List<ExternalCollectionEntry> _externalCollectionEntries = [];
        protected List<Action<EntityInitializedEvent>> _relationshipResolvers = [];

        // Use HashSet with custom equality comparer to prevent duplicate entities with the same key
        protected HashSet<IDataDrivenEntityBase> _allEntities = new(new EntityEqualityComparer());
        protected IInitializationContext? _initializationContext;
        protected bool _isResolvingRelationships = false;

        protected DataDrivenEntityBase()
        {
            IsNew = true;
        }

        public bool IsInitialized { get; protected set; }
        public bool IsNew { get; set; }
        protected abstract bool SelfInitialzed { get; }

        public IEnumerable<IDataDrivenEntityBase> GetAllEntities(bool includeSelf = false)
        {
            // Return a shallow copy to prevent modification of the internal collection
            return includeSelf ? _allEntities.ToList() : _allEntities.Where(e => !e.GetKey().Equals(GetKeyCore()));
        }

        // Explicit implementation of the interface methods
        // This allows derived classes to have methods with the same names
        object IDataDrivenEntityBase.GetEntityData() => GetEntityDataCore();
        object IDataDrivenEntityBase.GetKey() => GetKeyCore();

        // Protected abstract methods that derived classes must implement
        // These have different names to avoid conflicts
        protected abstract object GetEntityDataCore();
        protected abstract object GetKeyCore();

        protected virtual void ResolveRelationships(EntityInitializedEvent initEvent)
        {
            // Prevent recursive relationship resolution
            if (_isResolvingRelationships)
                return;

            try
            {
                _isResolvingRelationships = true;

                // Call all registered relationship resolvers
                foreach (var resolver in _relationshipResolvers)
                {
                    resolver(initEvent);
                }
            }
            finally
            {
                _isResolvingRelationships = false;
            }
        }

        protected void RegisterRelationshipResolver(Action<EntityInitializedEvent> resolver)
        {
            _relationshipResolvers.Add(resolver);
        }

        protected void UnregisterRelationshipResolver(Action<EntityInitializedEvent> resolver)
        {
            _relationshipResolvers.Remove(resolver);
        }

        protected void AddEntities(IDataDrivenEntityBase entity)
        {
            // Add the entity itself
            _allEntities.Add(entity);

            // Add its children, but avoid infinite recursion
            foreach (var child in entity.GetAllEntities())
            {
                if (child != entity) // Avoid adding entity twice
                {
                    _allEntities.Add(child);
                }
            }
        }

        protected void RemoveEntities(IDataDrivenEntityBase entity)
        {
            if (entity == null)
                return;

            // Remove the entity itself
            _allEntities.Remove(entity);

            // If the entity has its own child entities, also remove those that aren't used elsewhere
            var childEntities = entity.GetAllEntities();
            var entityComparer = new EntityEqualityComparer();

            foreach (var child in childEntities)
            {
                if (!entityComparer.Equals(child, entity)) // Avoid processing entity twice
                {
                    // Check if any other entity in our collection still references this child
                    bool isStillReferenced = false;

                    foreach (var otherEntity in _allEntities)
                    {
                        // Skip the entity being removed
                        if (entityComparer.Equals(otherEntity, entity))
                            continue;

                        // Check if the other entity references this child
                        if (otherEntity.GetAllEntities().Any(e => entityComparer.Equals(e, child)))
                        {
                            isStillReferenced = true;
                            break;
                        }
                    }

                    // Only remove the child if it's not referenced by any other entity
                    if (!isStillReferenced)
                    {
                        _allEntities.Remove(child);
                    }
                }
            }
        }

        // Common processing methods for entity initialization
        protected bool ProcessChildEntries<TEntityData>(TEntityData entityData, bool isNew, IInitializationContext context, bool initialized)
            where TEntityData : IEntityData<TKey>
        {
            foreach (var childEntry in _childEntries)
            {
                var childData = childEntry.ChildDataSelector(entityData);
                if (childEntry.Entity is not null)
                {
                    if (childData is null)
                    {
                        RemoveEntities(childEntry.Entity);
                        childEntry.RemoveChild(childEntry.Entity);
                        childEntry.Entity = null;
                    }
                    else
                    {
                        // Check if there's already an initialized version of this entity
                        var existingEntity = context.GetInitialized(
                            (childEntry.Entity.GetType(), childEntry.Entity.GetKey()));

                        if (existingEntity != null && !ReferenceEquals(existingEntity, childEntry.Entity))
                        {
                            // Use the already initialized entity
                            if (existingEntity is IDataDrivenEntity existingDataDrivenEntity)
                            {
                                childEntry.SetChild(existingDataDrivenEntity);
                                childEntry.Entity = existingDataDrivenEntity;
                                AddEntities(existingEntity);
                                initialized &= existingEntity.IsInitialized;
                            }
                        }
                        else
                        {
                            // Initialize the existing entity
                            bool childIsNew = !childData.GetKey().Equals(childEntry.Entity.GetKey());
                            var initEntity = childEntry.Entity;
                            initEntity.Initialize(childData, context, childIsNew || isNew);
                            AddEntities(childEntry.Entity);
                            initialized &= childEntry.Entity.IsInitialized;
                        }
                    }
                }
                else if (childData is not null)
                {
                    // Try to find an existing entity with the same key
                    var existingEntity = context.FindByKey(childData.GetKey());

                    if (existingEntity != null && existingEntity is IDataDrivenEntity existingDataDrivenEntity)
                    {
                        // Use existing entity
                        childEntry.SetChild(existingDataDrivenEntity);
                        childEntry.Entity = existingDataDrivenEntity;
                        AddEntities(existingEntity);
                        if (!existingEntity.IsInitialized)
                        {
                            existingDataDrivenEntity.Initialize(childData, context, false);
                        }
                        initialized &= existingEntity.IsInitialized;
                    }
                    else
                    {
                        // Create new entity
                        var newEntity = childEntry.ChildCreator(entityData);
                        // New entity is definitely new
                        var initEntity = newEntity;
                        initEntity.Initialize(childData, context, false);
                        AddEntities(newEntity);
                        childEntry.SetChild(newEntity);
                        childEntry.Entity = newEntity;
                        initialized &= newEntity.IsInitialized;
                    }
                }
            }

            return initialized;
        }

        protected bool ProcessExternalChildEntries<TEntityData>(TEntityData entityData, bool isNew, IInitializationContext context, bool initialized)
            where TEntityData : IEntityData<TKey>
        {
            foreach (var childEntry in _externalChildEntries)
            {
                var childData = childEntry.ChildDataSelector(entityData);
                var externalData = childEntry.ExternalDataProvider(entityData);
                if (childEntry.Entity is not null)
                {
                    if (childData is null)
                    {
                        RemoveEntities(childEntry.Entity);
                        childEntry.RemoveChild(childEntry.Entity);
                        childEntry.Entity = null;
                    }
                    else
                    {
                        // Check if there's already an initialized version of this entity
                        var existingEntity = context.GetInitialized(
                            (childEntry.Entity.GetType(), childEntry.Entity.GetKey()));

                        if (existingEntity != null && !ReferenceEquals(existingEntity, childEntry.Entity))
                        {
                            // Use the already initialized entity
                            if (existingEntity is IExternalDataDrivenEntity existingExternalEntity)
                            {
                                childEntry.SetChild(existingExternalEntity);
                                childEntry.Entity = existingExternalEntity;
                                AddEntities(existingEntity);
                                initialized &= existingEntity.IsInitialized;
                            }
                        }
                        else
                        {
                            // Initialize the existing entity
                            bool childIsNew = !childData.GetKey().Equals(childEntry.Entity.GetKey());
                            var initEntity = (IExternalDataDrivenEntity)childEntry.Entity;
                            initEntity.Initialize(childData, externalData, context, childIsNew || isNew);
                            AddEntities(childEntry.Entity);
                            initialized &= childEntry.Entity.IsInitialized;
                        }
                    }
                }
                else if (childData is not null)
                {
                    // Try to find an existing entity with the same key
                    var existingEntity = context.FindByKey(childData.GetKey());

                    if (existingEntity != null && existingEntity is IExternalDataDrivenEntity existingExternalEntity)
                    {
                        // Use existing entity
                        childEntry.SetChild(existingExternalEntity);
                        childEntry.Entity = existingExternalEntity;
                        AddEntities(existingEntity);
                        if (!existingEntity.IsInitialized)
                        {
                            existingExternalEntity.Initialize(entityData, externalData, context, false);
                        }
                        initialized &= existingEntity.IsInitialized;
                    }
                    else
                    {
                        // Create and initialize new entity
                        var newEntity = childEntry.ChildCreator(entityData);
                        // New entity is definitely new
                        var initEntity = newEntity;
                        initEntity.Initialize(childData, externalData, context, false);
                        childEntry.SetChild(newEntity);
                        childEntry.Entity = newEntity;
                        AddEntities(newEntity);
                        initialized &= newEntity.IsInitialized;
                    }
                }
            }

            return initialized;
        }

        protected bool ProcessChildCollections<TEntityData>(TEntityData entityData, bool isNew, IInitializationContext context, bool initialized)
            where TEntityData : IEntityData<TKey>
        {
            foreach (var collectionEntry in _collectionEntries)
            {
                var collectionData = collectionEntry.ChildCollectionDataSelector(entityData).ToList();
                var existingKeys = new List<object>();

                foreach (var childEntryData in collectionData)
                {
                    var key = childEntryData.GetKey();
                    existingKeys.Add(key);

                    // Try to find an existing entity first
                    var existingEntity = context.FindByKey(key);

                    if (existingEntity != null && existingEntity is IDataDrivenEntity existingDataDrivenEntity)
                    {
                        // Check if it's already in our collection
                        if (!collectionEntry.Collection.Contains(existingDataDrivenEntity))
                        {
                            collectionEntry.Collection.Add(existingDataDrivenEntity);
                        }

                        // Still initialize it to ensure it's up-to-date
                        bool entityIsNew = childEntryData.GetKey().Equals(existingDataDrivenEntity.GetKey());

                        existingDataDrivenEntity.Initialize(childEntryData, context, entityIsNew || isNew);
                        AddEntities(existingEntity);
                        initialized &= existingEntity.IsInitialized;
                    }
                    else
                    {
                        // Check in our collection first
                        var childEntry = collectionEntry.Collection.FirstOrDefault(e => e.GetKey().Equals(key));
                        if (childEntry is null)
                        {
                            childEntry = collectionEntry.ChildCreator(entityData, childEntryData);
                            collectionEntry.Collection.Add(childEntry);
                            var initEntity = childEntry;
                            initEntity.Initialize(childEntryData, context, false);
                        }
                        else
                        {
                            // Existing child may or may not be new
                            bool childIsNew = !childEntry.GetKey().Equals(childEntryData.GetKey());
                            var initEntity = childEntry;
                            initEntity.Initialize(childEntryData, context, childIsNew || isNew);
                        }

                        AddEntities(childEntry);
                        initialized &= childEntry.IsInitialized;
                    }
                }

                // Remove any collection items that are no longer in the data
                foreach (var entryToRemove in collectionEntry.Collection.Where(e => !existingKeys.Contains(e.GetKey())).ToArray())
                {
                    RemoveEntities(entryToRemove);
                    collectionEntry.Collection.Remove(entryToRemove);
                }
            }

            return initialized;
        }

        protected bool ProcessExternalChildCollections<TEntityData>(TEntityData entityData, bool isNew, IInitializationContext context, bool initialized)
    where TEntityData : IEntityData<TKey>
        {
            foreach (var collectionEntry in _externalCollectionEntries)
            {
                var collectionData = collectionEntry.ChildCollectionDataSelector(entityData).ToList();
                var externalData = collectionEntry.ExternalDataProvider(entityData);
                var existingKeys = new List<object>();

                foreach (var childEntryData in collectionData)
                {
                    var key = childEntryData.GetKey();
                    existingKeys.Add(key);

                    // Try to find an existing entity first
                    var existingEntity = context.FindByKey(key);

                    if (existingEntity != null && existingEntity is IExternalDataDrivenEntity existingExternalEntity)
                    {
                        // Check if it's already in our collection
                        if (!collectionEntry.Collection.Contains(existingExternalEntity))
                        {
                            collectionEntry.Collection.Add(existingExternalEntity);
                        }

                        // Still initialize it to ensure it's up-to-date
                        bool entityIsNew = !existingExternalEntity.GetKey().Equals(childEntryData.GetKey());
                        existingExternalEntity.Initialize(
                            childEntryData, externalData, context, entityIsNew || isNew);
                        AddEntities(existingEntity);
                        initialized &= existingEntity.IsInitialized;
                    }
                    else
                    {
                        // Create and add a new child entity FIRST
                        var childEntry = collectionEntry.ChildCreator(entityData, childEntryData);
                        collectionEntry.Collection.Add(childEntry);

                        // Then initialize it
                        var initEntity = childEntry;
                        initEntity.Initialize(childEntryData, externalData, context, false);

                        AddEntities(childEntry);
                        initialized &= childEntry.IsInitialized;
                    }
                }

                // Remove any collection items that are no longer in the data
                foreach (var entryToRemove in collectionEntry.Collection.Where(e => !existingKeys.Contains(e.GetKey())).ToArray())
                {
                    RemoveEntities(entryToRemove);
                    collectionEntry.Collection.Remove(entryToRemove);
                }
            }

            return initialized;
        }

        protected bool InitializeAggregateBase<TEntityData>(TEntityData entityData, bool isNew, IInitializationContext context)
            where TEntityData : IEntityData<TKey>
        {
            var initialized = true;
            initialized = ProcessChildEntries(entityData, isNew, context, initialized);
            initialized = ProcessExternalChildEntries(entityData, isNew, context, initialized);
            initialized = ProcessChildCollections(entityData, isNew, context, initialized);
            initialized = ProcessExternalChildCollections(entityData, isNew, context, initialized);

            return initialized;
        }
    }
}