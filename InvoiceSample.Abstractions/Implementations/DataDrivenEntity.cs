using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity.Implementations
{
    public abstract class DataDrivenEntity<TEntity, TKey, TEntityData> 
        : IDataDrivenEntity<TEntity, TKey, TEntityData>, IInitializable, IDataDrivenAggregate
        where TEntityData : IEntityData<TKey>
        where TEntity : new()
        where TKey : notnull
    {
        private bool _initialized;

        public abstract IEnumerable<IHasEntity> Children { get; }
        public abstract IEnumerable<IHasExternalEntity> ExternalChildren { get; }
        public abstract IEnumerable<IHasEntities> ChildrenCollections { get; }
        public abstract IEnumerable<IHasExternalEntities> ExternalChildrenCollections { get; }
        public abstract void SetChild(object selector, object? instance);
        public abstract IDataDrivenEntity GetChildInstance(object selector);
        public abstract TEntityData GetEntityData();
        public abstract TKey GetKey();
        public abstract void SelfInitialize(TEntityData entityData);
        public abstract bool IsInitialized();

        public bool Initialized() => _initialized && IsInitialized();

        public void Initialize(TEntityData entityData)
        {
            var initialized = false;

            SelfInitialize(entityData);
            if (HasAnyChildren)
            {
                initialized &= InitializeChildren(entityData);
            }
            if (HasAnyCollection)
            {
                initialized &= InitializeChildrenCollections();
            }

            _initialized = initialized;
        }

        void IInitializable.Initialize(object entityData)
        {
            if(entityData is TEntityData ed)
            {
                Initialize(ed);
            }
            else throw new InvalidCastException($"invalid entity data type - expecting {typeof(TEntityData).Name}");
        }

        object IInitializable.GetKey() => GetKey();

        private bool HasAnyChildren => Children.Any() || ExternalChildren.Any();

        private bool HasAnyCollection => ChildrenCollections.Any() || ExternalChildrenCollections.Any();

        private bool InitializeChildren(TEntityData entityData)
        {
            if(entityData is IHasEntityDatas childEntites)
            {
                foreach (var childEntityData in childEntites.Entities)
                {
                    var child = Children.SingleOrDefault(c => c.Selector.Equals(childEntityData.Selector));
                    var externalChild = ExternalChildren.SingleOrDefault(c => c.Selector.Equals(childEntityData.Selector));
                    if (child is not null && externalChild is not null)
                    {
                        throw new ArgumentException($"selector {childEntityData.Selector} for {childEntityData.GetKey()} in not unique");
                    }
                    else if (child is not null)
                    {
                        child.ChildEntity.Initialize(entityData);
                    }
                    else if (externalChild is not null) 
                    { 
                        var externalData = externalChild.GetExternalData(this, entityData);
                        if(externalData is null)
                        {
                            throw new ArgumentNullException($"external data was null for {externalChild.Selector}.{externalChild.ChildEntity.GetKey()}");
                        }

                        externalChild.ChildEntity.Initialize(entityData, externalData);
                    }
                    else
                    {
                        var newChild = GetChildInstance(childEntityData.Selector);
                        if (newChild is IInitializable initializable)
                        {
                            initializable.Initialize(entityData);
                        }
                        else if (newChild is IExternallyInitializable externallyInitializable)
                        {
                            var externalData = externallyInitializable.GetExternalData(this, entityData);
                            if (externalData is null)
                            {
                                throw new ArgumentNullException($"external data was null for {externalChild.Selector}.{externalChild.ChildEntity.GetKey()}");
                            }


                        }
                        else
                        {
                            throw new ArgumentException($"property {childEntityData.Selector} must be of Initializable type");
                        }
                    }
                }
            }
            else
            {
                throw new InvalidCastException("entity data expected to be of type IHasEntityDatas");
            }
        }

        void IDataDrivenEntity.Initialize(object entityData)
        {
            if(entityData is TEntityData ed)
            {
                Initialize(ed);
            }
            else
            {
                throw new InvalidCastException($"expecting {typeof(TEntityData).Name} got {entityData.GetType().Name}");
            }
        }

        object IDataDrivenEntity.GetEntityData()
        {
            return GetEntityData();
        }

        object IDataDrivenEntity.GetKey()
        {
            return GetKey();
        }
    }
}
