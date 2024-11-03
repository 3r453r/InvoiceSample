using AutoMapper;
using InvoiceSample.DataDrivenEntity;
using InvoiceSample.DataDrivenEntity.Aggregates;
using InvoiceSample.DataDrivenEntity.Implementations.Helpers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Persistence.Tables
{
    public class PPPBaseEntity
    {

    }

    public abstract class Entity<TEntity, TKey, TEntityData> : PPPBaseEntity
        , IDataDrivenEntity<TKey, TEntityData, IMapper>
        , IAggregateEntity<TKey, TEntityData>
        where TEntity : class, IDataDrivenEntity<TKey, TEntityData, IMapper>, TEntityData
        where TKey : notnull
        where TEntityData : IEntityData<TKey>
    {
        private List<ChildEntry> _childEntries = [];
        private List<ExternalChildEntry> _externalChildEntries = [];
        private List<DataDrivenEntity.Aggregates.CollectionEntry> _collectionEntries = [];
        private List<ExternalCollectionEntry> _externalCollectionEntries = [];
        private HashSet<IDataDrivenEntityBase> _allEntities = new();

        protected IMapper? _mapper;

        public Guid Id { get; set; } = NewId.NextSequentialGuid();
        public DateTime Created { get; set; } = DateTime.Now;

        [NotMapped]
        public bool IsInitialized { get; private set; }

        [NotMapped]
        public bool IsNew { get; set; }

        public IEnumerable<IDataDrivenEntityBase> GetAllEntities() => _allEntities;
        public abstract TEntityData GetEntityData();
        public abstract object GetKey();

        public void Initialize(TEntityData entityData, IMapper externalData, bool isNew = false)
        {
            _mapper = externalData;
            _mapper.Map(entityData, this);
            IsInitialized = InitializeAggregate(entityData);
        }

        public void Initialize(object entityData, object externalData, bool isNew = false)
        {
            if (entityData is TEntityData enD && externalData is IMapper exD)
            {
                Initialize(enD, exD, isNew);
            }
            else
            {
                throw new InvalidCastException($"expecting types {typeof(TEntityData).Name} and {typeof(IMapper).Name}");
            }
        }

        private bool InitializeAggregate(TEntityData entityData)
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
                        childEntry.Entity.Initialize(childData);
                        AddEntities(childEntry.Entity);
                        initialized &= childEntry.Entity.IsInitialized;
                    }
                }
                else if (childData is not null)
                {
                    var newEntity = childEntry.ChildCreator(entityData);
                    newEntity.Initialize(childData);
                    newEntity.IsNew = true;
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
                        childEntry.Entity.Initialize(entityData, externalData);
                        AddEntities(childEntry.Entity);
                        initialized &= childEntry.Entity.IsInitialized;
                    }
                }
                else if (childData is not null)
                {
                    var newEntity = childEntry.ChildCreator(entityData);
                    newEntity.Initialize(childData, externalData);
                    newEntity.IsNew = true;
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
                        childEntry.IsNew = true;
                        collectionEntry.Collection.Add(childEntry);
                    }
                    childEntry.Initialize(childEntryData);
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
                        childEntry.IsNew = true;
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

        object IDataDrivenEntityBase.GetEntityData() => GetEntityData();

        private void AddEntities(IDataDrivenEntityBase entity)
        {
            _allEntities.Add(entity);
            foreach (var child in entity.GetAllEntities())
            {
                _allEntities.Add(child);
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

        public void RegisterExternalChild<TChild, TChildKey, TChildData, TChildExternalData>(
            TChild? child
            , Func<TEntityData, TChildData?> childDataSelector
            , Action<IExternalDataDrivenEntity> removeChild
            , Action<IExternalDataDrivenEntity> setChild
            , Func<TEntityData, TChild> childCreator
            , Func<TEntityData, TChildExternalData> externalDataProvider)
            where TChild : IDataDrivenEntity<TChildKey, TChildData, TChildExternalData>
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
            _collectionEntries.Add(new DataDrivenEntity.Aggregates.CollectionEntry
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
