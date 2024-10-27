using InvoiceSample.DataDrivenEntity.Aggregates;
using InvoiceSample.DataDrivenEntity.HasEntity;
using InvoiceSample.DataDrivenEntity.Initializable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity.Implementations
{
    public abstract class ExternalDataDrivenEntity<TEntity, TKey, TEntityData, TExternalData>
        : IDataDrivenEntity<TEntity, TKey, TEntityData, TExternalData>, IAggregateEntity
        where TEntityData : IEntityData<TKey>
        where TEntity : new()
        where TKey : notnull
        where TExternalData : class
    {

        public bool IsInitialized { get; private set; }
        protected abstract bool SelfInitialzed { get; }
        public abstract IEnumerable<ChildEntry> ChildEntries { get; }
        public abstract IEnumerable<CollectionEntry> CollectionEntries { get; }

        public abstract TEntityData GetEntityData();
        object IExternalDataDrivenEntity.GetEntityData() => GetEntityData();

        public abstract TKey GetKey();
        protected abstract void SelfInitialize(TEntityData entityData, TExternalData externalData);
        protected abstract IInitializeBase CreateChild(string selector);
        public abstract void ClearChild(string selector);

        public abstract TExternalData? GetExternalData(TEntityData entityData);
        object? IExternalDataDrivenEntity.GetExternalData(object entityData)
        {
            if (entityData is TEntityData ed)
            {
                return GetExternalData(ed);
            }
            else
            {
                throw new InvalidCastException($"expecting data of type {typeof(TEntityData).Name}");
            }
        }

        public void Initialize(TEntityData entityData, TExternalData externalData)
        {
            SelfInitialize(entityData, externalData);
            IsInitialized = InitializeAggregate(entityData) && SelfInitialzed;
        }

        void IExternallyInitializable.Initialize(object entityData, object externalData)
        {
            if (entityData is TEntityData enD && externalData is TExternalData exD)
            {
                Initialize(enD, exD);
            }
            else
            {
                throw new InvalidCastException($"expecting types {typeof(TEntityData).Name} and {typeof(TExternalData).Name}");
            }
        }

        object IInitializeBase.GetKey() => GetKey();

        protected void InitializeChildEntity(IInitializeBase entity, IEntityData childEntityData, TEntityData parentEntityData)
        {
            if (entity is IDataDrivenEntity initializable)
            {
                initializable.Initialize(childEntityData);
            }
            else if (entity is IExternalDataDrivenEntity externallyInitializable)
            {
                var externalData = GetExternalData(parentEntityData);
                if (externalData == null)
                {
                    throw new ArgumentNullException($"External data was null for entity {entity.GetKey()} - parent {parentEntityData.GetKey()}");
                }

                externallyInitializable.Initialize(childEntityData, externalData);
            }
            else
            {
                throw new ArgumentException($"Entity must be IDataDrivenEntity or IExternalDataDrivenEntity");
            }
        }

        private bool InitializeAggregate(TEntityData entityData)
        {
            var initialized = true;

            if (entityData is IAggregateEntityData aggregateData)
            {
                CheckSelectorUniqueness();

                // Initialize Child Entities based on data
                foreach (var childData in aggregateData.ChildrenData)
                {
                    var selector = childData.Selector;
                    var childEntityData = childData.Entity;

                    if (childEntityData != null)
                    {
                        var childEntry = ChildEntries.SingleOrDefault(c => c.Selector == selector);

                        IInitializeBase entity;

                        if (childEntry != null)
                        {
                            entity = childEntry.Entity ?? CreateChild(selector);

                            // Update the ChildEntry's entity if it was previously null
                            if (childEntry.Entity == null)
                            {
                                childEntry.Entity = entity;
                            }
                        }
                        else
                        {
                            throw new ArgumentNullException($"childEntry was null for selector {selector}");
                        }

                        InitializeChildEntity(entity, childEntityData, entityData);

                        initialized &= entity.IsInitialized;
                    }
                    else
                    {
                        // If the child data's entity is null, clear the child
                        ClearChild(selector);
                    }
                }

                // Initialize Collection Entities based on data
                foreach (var collectionData in aggregateData.ChildrenCollectionsData)
                {
                    var collectionSelector = collectionData.Selector;
                    var collectionEntry = CollectionEntries.SingleOrDefault(c => c.CollectionSelector == collectionSelector);

                    if (collectionEntry != null)
                    {
                        var existingKeys = new List<object>();

                        foreach (var childData in collectionData.Collection)
                        {
                            var key = childData.GetKey();
                            existingKeys.Add(key);

                            var entity = collectionEntry.Collection.FirstOrDefault(c => c.GetKey().Equals(key));

                            if (entity == null)
                            {
                                entity = CreateChild(collectionEntry.ChildSelector);
                                collectionEntry.Collection.Add(entity);
                            }

                            InitializeChildEntity(entity, childData, entityData);

                            initialized &= entity.IsInitialized;
                        }

                        // Remove entities not in existing keys
                        var entitiesToRemove = collectionEntry.Collection.Where(c => !existingKeys.Contains(c.GetKey())).ToList();
                        foreach (var entityToRemove in entitiesToRemove)
                        {
                            collectionEntry.Collection.Remove(entityToRemove);
                        }
                    }
                    else
                    {
                        throw new ArgumentNullException($"collectionEntry was null for selector {collectionSelector}");
                    }
                }
            }
            else
            {
                throw new InvalidOperationException($"Expecting data of type {typeof(IAggregateEntityData).Name}");
            }

            return initialized;
        }

        private void CheckSelectorUniqueness()
        {
            var entitySelectors = ChildEntries.Select(c => c.Selector)
                .Union(CollectionEntries.Select(c => c.ChildSelector));

            var selectorSet = new HashSet<string>();
            foreach (var selector in entitySelectors)
            {
                if (!selectorSet.Add(selector))
                {
                    throw new Exception($"Duplicate entity selector found: {selector}");
                }
            }

            selectorSet.Clear();
            foreach (var selector in CollectionEntries.Select(c => c.CollectionSelector))
            {
                if (!selectorSet.Add(selector))
                {
                    throw new Exception($"Duplicate collection selector found: {selector}");
                }
            }
        }
    }
}
