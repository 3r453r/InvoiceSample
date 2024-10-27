using InvoiceSample.DataDrivenEntity.Aggregates;
using InvoiceSample.DataDrivenEntity.Implementations.External;
using InvoiceSample.DataDrivenEntity.Initializable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity.Implementations
{
    public abstract class ReflectiveSelfDataExternalDataDrivenEntity<TEntity, TKey, TEntityData, TExternalData>
    : SelfDataExternalDataDrivenEntity<TEntity, TKey, TEntityData, TExternalData>
        where TEntity : TEntityData, new()
        where TEntityData : IEntityData<TKey>
        where TKey : notnull
        where TExternalData : class
    {
        // Fields to cache reflection results
        private readonly Lazy<List<ChildEntry>> _lazyChildEntries;
        private readonly Lazy<List<CollectionEntry>> _lazyCollectionEntries;

        private readonly Dictionary<string, Func<IInitializeBase>> _childCreators = new();
        private readonly Dictionary<string, Action> _childClearActions = new();

        private bool _selfInitialized;

        protected ReflectiveSelfDataExternalDataDrivenEntity()
        {
            _lazyChildEntries = new Lazy<List<ChildEntry>>(InitializeChildEntries);
            _lazyCollectionEntries = new Lazy<List<CollectionEntry>>(InitializeCollectionEntries);
        }

        public override TKey GetKey()
        {
            return ((IEntityData<TKey>)this).GetKey();
        }

        public override IEnumerable<ChildEntry> ChildEntries => _lazyChildEntries.Value;
        public override IEnumerable<CollectionEntry> CollectionEntries => _lazyCollectionEntries.Value;

        protected override IInitializeBase CreateChild(string selector)
        {
            if (_childCreators.TryGetValue(selector, out var creator))
            {
                return creator();
            }
            else
            {
                throw new InvalidOperationException($"No child creator found for selector: {selector}");
            }
        }

        public override void ClearChild(string selector)
        {
            if (_childClearActions.TryGetValue(selector, out var clearAction))
            {
                clearAction();
            }
            else
            {
                throw new InvalidOperationException($"No clear action found for selector: {selector}");
            }
        }

        private List<ChildEntry> InitializeChildEntries()
        {
            var childEntries = new List<ChildEntry>();

            var interfaces = this.GetType().GetInterfaces();

            foreach (var iface in interfaces)
            {
                if (iface.IsGenericType)
                {
                    var genericTypeDefinition = iface.GetGenericTypeDefinition();

                    // Handle IHasChild<TSelfData, TSelfKey, TEntity, TChildKey, TChildEntityData, TChild>
                    if (genericTypeDefinition.Name == "IHasChild`6")
                    {
                        var typeArguments = iface.GetGenericArguments();

                        // Extract TChild (6th generic parameter)
                        var childType = typeArguments[5];

                        // Get the 'Child' property
                        var childProperty = this.GetType().GetProperty("Child", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                        if (childProperty != null && childProperty.PropertyType == childType)
                        {
                            var selector = childProperty.Name;

                            var childEntity = (IInitializeBase?)childProperty.GetValue(this);

                            var childEntry = new ChildEntry(childEntity, selector);

                            childEntries.Add(childEntry);

                            // Store the creator and clear action
                            _childCreators[selector] = () =>
                            {
                                var childInstance = (IInitializeBase)Activator.CreateInstance(childType)!;

                                // Set the child property
                                childProperty.SetValue(this, childInstance);

                                return childInstance;
                            };

                            _childClearActions[selector] = () =>
                            {
                                // Clear the child property
                                childProperty.SetValue(this, null);
                            };
                        }
                    }

                    // Handle IHasMultipleEntityInstances<TChildEntity, ..., TChildInstanceSelector>
                    else if (genericTypeDefinition.Name == "IHasMultipleEntityInstances`7")
                    {
                        var typeArguments = iface.GetGenericArguments();

                        // Extract TChildEntity and TChildInstanceSelector
                        var childEntityType = typeArguments[0];
                        var childInstanceSelectorType = typeArguments[6];

                        // Get the 'ChildInstances' property
                        var childInstancesProperty = this.GetType().GetProperty("ChildInstances", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                        if (childInstancesProperty != null)
                        {
                            var childInstancesValue = childInstancesProperty.GetValue(this);

                            // Cast to IEnumerable
                            var instances = (IEnumerable)childInstancesValue;

                            foreach (var item in instances)
                            {
                                // Each item is a tuple: (TChildEntity? Entity, TChildInstanceSelector Selector)

                                // Get properties 'Entity' and 'Selector' using reflection
                                var itemType = item.GetType();
                                var entityProperty = itemType.GetProperty("Entity");
                                var selectorProperty = itemType.GetProperty("Selector");

                                var entity = (IInitializeBase?)entityProperty.GetValue(item);
                                var selector = selectorProperty.GetValue(item)?.ToString() ?? "";

                                var childEntry = new ChildEntry(entity, selector);

                                childEntries.Add(childEntry);

                                // Store the child creator
                                _childCreators[selector] = () =>
                                {
                                    var childInstance = (IInitializeBase)Activator.CreateInstance(childEntityType)!;

                                    // Set the Entity property
                                    entityProperty.SetValue(item, childInstance);

                                    return childInstance;
                                };

                                // Store the clear action
                                _childClearActions[selector] = () =>
                                {
                                    // Set the Entity property to null
                                    entityProperty.SetValue(item, null);
                                };
                            }
                        }
                    }
                }
            }

            return childEntries;
        }

        private List<CollectionEntry> InitializeCollectionEntries()
        {
            var collectionEntries = new List<CollectionEntry>();

            var interfaces = this.GetType().GetInterfaces();

            foreach (var iface in interfaces)
            {
                if (iface.IsGenericType)
                {
                    var genericTypeDefinition = iface.GetGenericTypeDefinition();

                    // Handle IHasChildren<TChildEntity, TSelfEntityData, TSelfKey, TEntity, TChildKey, TChildEntityData>
                    if (genericTypeDefinition.Name == "IHasChildren`6")
                    {
                        var typeArguments = iface.GetGenericArguments();

                        // Extract TChildEntity
                        var childEntityType = typeArguments[0];

                        // Get the 'ChildEntities' property
                        var childrenProperty = this.GetType().GetProperty("ChildEntities", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                        if (childrenProperty != null)
                        {
                            var collectionSelector = childrenProperty.Name;
                            var childSelector = childEntityType.Name; // Adjust as needed

                            // Get the collection instance
                            var collection = (ICollection<IInitializeBase>)childrenProperty.GetValue(this)!;

                            var collectionEntry = new CollectionEntry(collection, collectionSelector, childSelector);

                            collectionEntries.Add(collectionEntry);

                            // Store the child creator
                            _childCreators[childSelector] = () =>
                            {
                                var childInstance = (IInitializeBase)Activator.CreateInstance(childEntityType)!;

                                // Addition to the collection happens elsewhere

                                return childInstance;
                            };

                            // Optionally, handle clear actions
                        }
                    }

                    // Handle IHasMultipleEntityCollections<TChildEntity, ..., TCollectionSelector>
                    else if (genericTypeDefinition.Name == "IHasMultipleEntityCollections`7")
                    {
                        var typeArguments = iface.GetGenericArguments();

                        // Extract TChildEntity and TCollectionSelector
                        var childEntityType = typeArguments[0];
                        var collectionSelectorType = typeArguments[6]; // 7th generic parameter

                        // Get the 'ChildEntityCollections' property
                        var childEntityCollectionsProperty = this.GetType().GetProperty("ChildEntityCollections", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                        if (childEntityCollectionsProperty != null)
                        {
                            var childEntityCollectionsValue = childEntityCollectionsProperty.GetValue(this);

                            // Cast to IEnumerable
                            var collections = (IEnumerable)childEntityCollectionsValue;

                            foreach (var item in collections)
                            {
                                // Each item is a tuple: (IEnumerable<TChildEntity> Collection, TCollectionSelector Selector)

                                // Get properties 'Collection' and 'Selector' using reflection
                                var itemType = item.GetType();
                                var collectionProperty = itemType.GetProperty("Collection");
                                var selectorProperty = itemType.GetProperty("Selector");

                                var collection = (ICollection<IInitializeBase>)collectionProperty.GetValue(item)!;
                                var collectionSelector = selectorProperty.GetValue(item)?.ToString() ?? "";

                                var childSelector = childEntityType.Name; // Adjust as needed

                                var collectionEntry = new CollectionEntry(collection, collectionSelector, childSelector);

                                collectionEntries.Add(collectionEntry);

                                // Store the child creator
                                _childCreators[childSelector] = () =>
                                {
                                    var childInstance = (IInitializeBase)Activator.CreateInstance(childEntityType)!;

                                    // Addition to the collection happens elsewhere

                                    return childInstance;
                                };

                                // Optionally, handle clear actions
                            }
                        }
                    }
                }
            }

            return collectionEntries;
        }

        // Implement InitializeChildEntity
        protected void InitializeChildEntity(IInitializeBase entity, IEntityData childEntityData, TEntity parentEntityData)
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

        // Optionally implement other methods or override existing ones
    }
}
