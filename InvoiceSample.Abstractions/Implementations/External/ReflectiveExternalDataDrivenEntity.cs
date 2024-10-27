using InvoiceSample.DataDrivenEntity.Aggregates;
using InvoiceSample.DataDrivenEntity.HasEntity;
using InvoiceSample.DataDrivenEntity.Initializable;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity.Implementations.External
{
    public abstract class ReflectiveExternalDataDrivenEntity<TEntity, TKey, TEntityData, TExternalData>
    : ExternalDataDrivenEntity<TEntity, TKey, TEntityData, TExternalData>
        where TEntity : new()
        where TEntityData : IEntityData<TKey>
        where TKey : notnull
        where TExternalData : class
    {
        // Lazy fields to cache reflection results
        private readonly Lazy<List<ChildEntry>> _lazyChildEntries;
        private readonly Lazy<List<CollectionEntry>> _lazyCollectionEntries;

        // Dictionaries to hold creators and clear actions
        private readonly Dictionary<string, Func<IInitializeBase>> _childCreators = new();
        private readonly Dictionary<string, Action> _childClearActions = new();

        protected ReflectiveExternalDataDrivenEntity()
        {
            // Initialize Lazy<T> fields
            _lazyChildEntries = new Lazy<List<ChildEntry>>(InitializeChildEntries);
            _lazyCollectionEntries = new Lazy<List<CollectionEntry>>(InitializeCollectionEntries);
        }

        // Override properties to use the cached Lazy<T> values
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

        // Method to initialize ChildEntries using reflection
        private List<ChildEntry> InitializeChildEntries()
        {
            var childEntries = new List<ChildEntry>();

            var interfaces = GetType().GetInterfaces();

            foreach (var iface in interfaces)
            {
                if (iface.IsGenericType)
                {
                    var genericTypeDefinition = iface.GetGenericTypeDefinition();

                    // Adjust for your IHasChild interface with multiple type parameters
                    if (genericTypeDefinition.Name == "IHasChild`6")
                    {
                        var typeArguments = iface.GetGenericArguments();
                        var childType = typeArguments[5]; // TChild is the sixth type parameter

                        // Get the 'Child' property
                        var childProperty = GetType().GetProperty("Child", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

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
                                childProperty.SetValue(this, childInstance);
                                return childInstance;
                            };

                            _childClearActions[selector] = () =>
                            {
                                childProperty.SetValue(this, null);
                            };
                        }
                    }
                }
            }

            return childEntries;
        }

        // Method to initialize CollectionEntries using reflection
        private List<CollectionEntry> InitializeCollectionEntries()
        {
            var collectionEntries = new List<CollectionEntry>();

            // Ensure _lazyChildEntries is initialized
            var childEntries = _lazyChildEntries.Value;

            var interfaces = GetType().GetInterfaces();

            foreach (var iface in interfaces)
            {
                if (iface.IsGenericType)
                {
                    var genericTypeDefinition = iface.GetGenericTypeDefinition();

                    // Handle IHasChildren<TChildEntity, TSelfEntityData, TSelfKey, TEntity, TChildKey, TChildEntityData>
                    if (genericTypeDefinition.Name == "IHasChildren`6")
                    {
                        // Existing code to handle IHasChildren
                    }
                    // Handle IHasMultipleEntityCollections<TChildEntity, ..., TCollectionSelector>
                    else if (genericTypeDefinition.Name == "IHasMultipleEntityCollections`7")
                    {
                        // Existing code to handle IHasMultipleEntityCollections
                    }
                    // Handle IHasMultipleEntityInstances<TChildEntity, ..., TChildInstanceSelector>
                    else if (genericTypeDefinition.Name == "IHasMultipleEntityInstances`7")
                    {
                        var typeArguments = iface.GetGenericArguments();

                        // Extract TChildEntity and TChildInstanceSelector
                        var childEntityType = typeArguments[0];
                        var instanceSelectorType = typeArguments[6]; // TChildInstanceSelector is the 7th type parameter

                        // Get the 'ChildInstances' property
                        var childInstancesProperty = GetType().GetProperty("ChildInstances", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

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

                                // Add to _lazyChildEntries.Value
                                _lazyChildEntries.Value.Add(childEntry);

                                // Store the child creator
                                _childCreators[selector] = () =>
                                {
                                    var childInstance = (IInitializeBase)Activator.CreateInstance(childEntityType)!;

                                    // Set the Entity property
                                    entityProperty.SetValue(item, childInstance);

                                    return childInstance;
                                };

                                // Optionally, handle clear actions
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

            return collectionEntries;
        }

        public abstract override TKey GetKey();
        protected abstract override void SelfInitialize(TEntityData entityData, TExternalData externalData);
        public abstract override TExternalData? GetExternalData(TEntityData entityData);
    }
}
