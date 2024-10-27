using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InvoiceSample.DataDrivenEntity.Aggregates;
using InvoiceSample.DataDrivenEntity.HasEntity;
using InvoiceSample.DataDrivenEntity.Initializable;

namespace InvoiceSample.DataDrivenEntity.Implementations.Basic
{
    public abstract class DataDrivenEntity<TEntity, TKey, TEntityData>
        : IDataDrivenEntity<TEntity, TKey, TEntityData>, IAggregateEntity
        where TEntityData : IEntityData<TKey>
        where TEntity : new()
        where TKey : notnull
    {

        public bool IsInitialized { get; private set; }
        protected abstract bool SelfInitialzed { get; }
        public abstract IEnumerable<ChildEntry> ChildEntries { get; }
        public abstract IEnumerable<CollectionEntry> CollectionEntries { get; }
        public abstract TEntityData GetEntityData();
        public abstract TKey GetKey();
        protected abstract void SelfInitialize(TEntityData entityData);
        protected abstract IInitializeBase CreateChild(string selector);
        public abstract void ClearChild(string selector);

        object IDataDrivenEntity.GetEntityData() => GetEntityData();
        object IInitializeBase.GetKey() => GetKey();

        public void Initialize(TEntityData entityData)
        {
            SelfInitialize(entityData);
            IsInitialized = InitializeAggregate(entityData) && SelfInitialzed;
        }

        void IInitializable.Initialize(object entityData)
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

        private bool InitializeAggregate(TEntityData entityData)
        {
            var initialized = false;
            if (entityData is IAggregateEntityData aggregateData)
            {
                CheckSelectorUniqueness();
                foreach (var childData in aggregateData.ChildrenData)
                {
                    var childEntityData = childData.Entity;
                    if (childEntityData is not null)
                    {
                        var childEntry = ChildEntries.Single(c => c.Selector == childData.Selector);
                        var entity = childEntry.Entity;
                        if (childEntry.Entity is null)
                        {
                            entity = CreateChild(childEntry.Selector);
                        }
                        if (entity is IDataDrivenEntity initializable)
                        {
                            initializable.Initialize(childEntityData);
                        }
                        else if (entity is IExternalDataDrivenEntity externallyInitializable)
                        {
                            var externalData = ((IHasExternalChild)this)
                                .GetExternalData(externallyInitializable, entityData);
                            if (externalData is null)
                            {
                                throw new ArgumentNullException($"external data was null for {childData.Selector} - {entityData.GetKey()}");
                            }

                            externallyInitializable.Initialize(childEntityData, externalData);
                        }
                        else
                        {
                            throw new ArgumentException($"{childData.Selector} - {entityData.GetKey()} mus be IInitializable");
                        }

                        initialized &= entity.IsInitialized;
                    }
                    else
                    {
                        ClearChild(childData.Selector);
                    }
                }

                foreach (var childCollectionData in aggregateData.ChildrenCollectionsData)
                {
                    var childCollectionEntry = CollectionEntries
                        .Single(c => c.CollectionSelector == childCollectionData.Selector);
                    List<object> existingKeys = [];
                    foreach (var childData in childCollectionData.Collection)
                    {
                        var key = childData.GetKey();
                        existingKeys.Add(key);

                        var entity = childCollectionEntry.Collection
                            .FirstOrDefault(c => c.GetKey().Equals(key));
                        if (entity is null)
                        {
                            entity = CreateChild(childCollectionEntry.ChildSelector);
                            childCollectionEntry.Collection.Add(entity);
                        }
                        if (entity is IDataDrivenEntity initializable)
                        {
                            initializable.Initialize(childData);
                        }
                        else if (entity is IExternalDataDrivenEntity externallyInitializable)
                        {
                            var externalData = ((IHasExternalChild)this)
                                .GetExternalData(externallyInitializable, entityData);
                            if (externalData is null)
                            {
                                throw new ArgumentNullException($"external data was null for {childCollectionData.Selector} - {childData.GetKey()}");
                            }

                            externallyInitializable.Initialize(childData, externalData);
                        }
                        else
                        {
                            throw new ArgumentException($"{childCollectionData.Selector} - {entityData.GetKey()} mus be IInitializable");
                        }

                        initialized &= entity.IsInitialized;
                    }

                    foreach (var childToRemove in childCollectionEntry.Collection.Where(c => !existingKeys.Contains(c.GetKey())).ToArray())
                    {
                        CollectionEntries.First(ce => ce.CollectionSelector == childCollectionData.Selector)
                            .Collection.Remove(childToRemove);
                    }
                }
            }
            else
            {
                throw new InvalidOperationException($"expecting data of type {typeof(IAggregateEntityData).Name}");
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
