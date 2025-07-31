using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity.Implementations.Helpers
{
    public class InitializationContext : IInitializationContext
    {
        private readonly Dictionary<(Type EntityType, object Key), IDataDrivenEntityBase> _initializedEntities = new();
        private readonly List<EntityInitializedEvent> _initializationEvents = new();
        private readonly List<Action<EntityInitializedEvent>> _eventSubscribers = new();
        private bool _notifyingSubscribers = false;

        public bool IsInitialized(IDataDrivenEntityBase entity)
        {
            if (entity == null)
                return false;

            return _initializedEntities.ContainsKey((entity.GetType(), entity.GetKey()));
        }

        public void Add(IDataDrivenEntityBase entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var key = (entity.GetType(), entity.GetKey());
            if (_initializedEntities.ContainsKey(key))
                return; // Entity already tracked, avoid duplicate tracking

            _initializedEntities.Add(key, entity);

            // Raise event for subscribers
            var initEvent = new EntityInitializedEvent(entity);
            _initializationEvents.Add(initEvent);

            // Notify subscribers if we're not already in the middle of notifying
            // This prevents potential infinite recursion if subscribers call Add
            if (!_notifyingSubscribers)
            {
                NotifySubscribers(initEvent);
            }
        }

        private void NotifySubscribers(EntityInitializedEvent initEvent)
        {
            if (_eventSubscribers.Count == 0) return;

            try
            {
                _notifyingSubscribers = true;
                foreach (var subscriber in _eventSubscribers.ToList()) // Create a copy to avoid modification issues
                {
                    subscriber(initEvent);
                }
            }
            finally
            {
                _notifyingSubscribers = false;
            }
        }

        public IDataDrivenEntityBase? GetInitialized((Type EntityType, object Key) key)
        {
            if (_initializedEntities.TryGetValue(key, out var entry))
            {
                return entry;
            }

            // Search by derivation - try to find entities that are compatible with the requested type
            foreach (var pair in _initializedEntities)
            {
                if (pair.Key.Key.Equals(key.Key) && key.EntityType.IsAssignableFrom(pair.Key.EntityType))
                {
                    return pair.Value;
                }
            }

            return null;
        }

        public IDataDrivenEntityBase? FindByKey<TKey>(TKey key) where TKey : notnull
        {
            foreach (var pair in _initializedEntities)
            {
                if (pair.Key.Key.Equals(key))
                {
                    return pair.Value;
                }
            }

            return null;
        }

        public IEnumerable<IDataDrivenEntityBase> GetAllInitializedEntities()
        {
            return _initializedEntities.Values.ToList();
        }

        public void SubscribeToInitializationEvents(Action<EntityInitializedEvent> subscriber)
        {
            if (_eventSubscribers.Contains(subscriber))
                return;

            _eventSubscribers.Add(subscriber);

            // Notify about previous events - this is needed for tests that expect retroactive notifications
            foreach (var pastEvent in _initializationEvents.ToList())
            {
                try
                {
                    subscriber(pastEvent);
                }
                catch (Exception ex)
                {
                    // Log or handle exceptions during past event replay
                    System.Diagnostics.Debug.WriteLine($"Error notifying subscriber about past event: {ex.Message}");
                }
            }
        }

        public void UnsubscribeFromInitializationEvents(Action<EntityInitializedEvent> subscriber)
        {
            _eventSubscribers.Remove(subscriber);
        }
    }

    public class EntityInitializedEvent
    {
        public IDataDrivenEntityBase Entity { get; }
        public DateTime Timestamp { get; }

        public EntityInitializedEvent(IDataDrivenEntityBase entity)
        {
            Entity = entity;
            Timestamp = DateTime.UtcNow;
        }
    }
}