using InvoiceSample.DataDrivenEntity.Aggregates;
using InvoiceSample.DataDrivenEntity.HasEntity;
using InvoiceSample.DataDrivenEntity.Implementations.External;
using InvoiceSample.DataDrivenEntity.Implementations.Helpers;
using InvoiceSample.DataDrivenEntity.Initializable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity.Implementations.Basic
{
    public abstract class ReflectiveSelfDataDataDrivenEntity<TEntity, TKey, TEntityData> : SelfDataDataDrivenEntity<TEntity, TKey, TEntityData>
    where TEntity : TEntityData, new()
    where TKey : notnull
        where TEntityData : IEntityData<TKey>
    {
        private readonly Lazy<List<ChildEntry>> _lazyChildEntries;
        private readonly Lazy<List<CollectionEntry>> _lazyCollectionEntries;
        public override IEnumerable<ChildEntry> ChildEntries => _lazyChildEntries.Value;
        public override IEnumerable<CollectionEntry> CollectionEntries => _lazyCollectionEntries.Value;

        //private readonly List<ChildEntry> _childEntries;
        //private readonly List<CollectionEntry> _collectionEntries;
        //public override IEnumerable<ChildEntry> ChildEntries => _childEntries;
        //public override IEnumerable<CollectionEntry> CollectionEntries => _collectionEntries;

        private readonly Dictionary<string, Func<IInitializeBase>> _childCreators = new();
        private readonly Dictionary<string, Action> _childClearActions = new();

        private bool _selfInitialized;

        protected ReflectiveSelfDataDataDrivenEntity()
        {
            //_childEntries = InitializeChildEntries();
            //_collectionEntries = InitializeCollectionEntries();
            _lazyChildEntries = new Lazy<List<ChildEntry>>(InitializeChildEntries);
            _lazyCollectionEntries = new Lazy<List<CollectionEntry>>(InitializeCollectionEntries);
        }

        public override TKey GetKey()
        {
            return ((IEntityData<TKey>)this).GetKey();
        }

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

            var interfaces = GetType().GetInterfaces();

            foreach (var iface in interfaces)
            {
                if (iface.IsGenericType)
                {
                    var genericTypeDefinition = iface.GetGenericTypeDefinition();

                    // Handle IHasChild<TSelfData, TSelfKey, TChild, TChildKey, TChildEntityData, TEntity>
                    if (genericTypeDefinition.Name == "IHasChild`6")
                    {
                        var typeArguments = iface.GetGenericArguments();

                        // Extract TChild (3rd generic parameter)
                        var childType = typeArguments[2];

                        // Get the 'Child' property
                        var childProperty = iface.GetProperty("Child", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                        if (childProperty != null && childProperty.PropertyType == childType)
                        {
                            var selector = GetType().GetProperties().Single(p => p.PropertyType == childType).Name;

                            var childEntity = (IInitializeBase?)childProperty.GetValue(this);

                            var childEntry = new ChildEntry(childEntity, selector);

                            childEntries.Add(childEntry);

                            _childCreators.Add(selector, () =>
                            {
                                var childInstance = (IInitializeBase)Activator.CreateInstance(childType)!;

                                // Set the child property
                                childProperty.SetValue(this, childInstance);

                                return childInstance;
                            });

                            _childClearActions.Add(selector, () =>                   
                            {
                                // Clear the child property
                                childProperty.SetValue(this, null);
                            });
                        }
                    }

                    // Handle IHasMultipleEntityInstances<TChildEntity, ..., TChildInstanceSelector>
                    else if (genericTypeDefinition.Name == "IHasMultipleEntityInstances`7")
                    {
                        var typeArguments = iface.GetGenericArguments();

                        // Extract TChildEntity and TChildInstanceSelector
                        var childEntityType = typeArguments[2];

                        //var childInstanceSelectorType = typeArguments[6];

                        // Get the 'ChildInstances' property
                        var childInstancesProperty = iface.GetProperty("ChildInstances", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                        //var childInstancesProperty = genericTypeDefinition.GetProperty("ChildInstances", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

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
                                var entityProperty = itemType.GetField("Item1");
                                var selectorProperty = itemType.GetField("Item2");

                                var entity = entityProperty.GetValue(item);
                                var selector = (string)selectorProperty.GetValue(item);

                                if(entity != null)
                                {
                                    childEntries.Add(new ChildEntry((IInitializeBase)entity, selector));
                                }
                                else
                                {
                                    childEntries.Add(new ChildEntry(null, selector));
                                }

                                

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
                        var childEntityType = typeArguments[2];
                        // Ensure TChildEntity implements IInitializeBase
                        if (!typeof(IInitializeBase).IsAssignableFrom(childEntityType))
                        {
                            throw new InvalidOperationException($"Type {childEntityType} must implement IInitializeBase.");
                        }
                        // Get the 'ChildEntities' property
                        var childrenProperty = iface.GetProperty("ChildEntities", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                        if (childrenProperty != null)
                        {
                            var collectionSelector = GetType().GetProperties().Single(p => p.PropertyType.I);
                            var childSelector = childEntityType.Name; // Adjust as needed

                            // Get the collection instance
                            var collectionValue = childrenProperty.GetValue(this);
                            // Create the wrapper
                            var wrapperType = typeof(CollectionWrapper<>).MakeGenericType(childEntityType);
                            var wrapper = (ICollection<IInitializeBase>)Activator.CreateInstance(wrapperType, collectionValue);

                            var collectionEntry = new CollectionEntry(wrapper, collectionSelector, childSelector);

                            collectionEntries.Add(collectionEntry);

                            _childCreators.Add(childSelector, () =>
                            {
                                var childInstance = (IInitializeBase)Activator.CreateInstance(childEntityType)!;

                                // Addition to the collection happens elsewhere

                                return childInstance;
                            });
                            // Optionally, handle clear actions
                        }
                    }

                    // Handle IHasMultipleEntityCollections<TChildEntity, ..., TCollectionSelector>
                    else if (genericTypeDefinition.Name == "IHasMultipleEntityCollections`7")
                    {
                        var typeArguments = iface.GetGenericArguments();

                        // Extract TChildEntity and TCollectionSelector
                        var childEntityType = typeArguments[2];

                        //var collectionSelectorType = typeArguments[6]; // 7th generic parameter

                        // Get the 'ChildEntityCollections' property
                        var childEntityCollectionsProperty = iface.GetProperty("ChildEntityCollections", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

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
                                var collectionProperty = itemType.GetField("Item1");
                                var selectorProperty = itemType.GetField("Item2");

                                // Get the collection instance
                                var collectionValue = collectionProperty.GetValue(item);
                                // Create the wrapper
                                var wrapperType = typeof(CollectionWrapper<>).MakeGenericType(childEntityType);
                                var wrapper = (ICollection<IInitializeBase>)Activator.CreateInstance(wrapperType, collectionValue);

                                var collectionSelector = (string)selectorProperty.GetValue(item);

                                var childSelector = childEntityType.Name; // Adjust as needed

                                var collectionEntry = new CollectionEntry(wrapper, collectionSelector, childSelector);

                                collectionEntries.Add(collectionEntry);

                                // Store the child creator
                                _childCreators.TryAdd(childSelector, () =>
                                {
                                    var childInstance = (IInitializeBase)Activator.CreateInstance(childEntityType)!;

                                    // Addition to the collection happens elsewhere

                                    return childInstance;
                                });

                                // Optionally, handle clear actions
                            }
                        }
                    }
                }
            }

            return collectionEntries;
        }

        private Type? GetCollectionItemType(PropertyInfo propertyInfo)
        {
            Type propertyType = propertyInfo.PropertyType;

            // Handle arrays
            if (propertyType.IsArray)
            {
                return propertyType.GetElementType();
            }

            // Handle generic collections
            if (propertyType.IsGenericType)
            {
                Type genericTypeDefinition = propertyType.GetGenericTypeDefinition();

                // Check if the type is a generic collection type
                if (typeof(IEnumerable<>).IsAssignableFrom(genericTypeDefinition))
                {
                    return propertyType.GetGenericArguments()[0];
                }

                // Check interfaces for IEnumerable<T>
                Type ienumerableInterface = propertyType.GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

                if (ienumerableInterface != null)
                {
                    return ienumerableInterface.GetGenericArguments()[0];
                }
            }

            // Handle interfaces that implement IEnumerable<T>
            foreach (Type interfaceType in propertyType.GetInterfaces())
            {
                if (interfaceType.IsGenericType &&
                    interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    return interfaceType.GetGenericArguments()[0];
                }
            }

            // Non-generic collections - item type cannot be determined
            return null;
        }
    }
}
