using InvoiceSample.DataDrivenEntity.Implementations.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace InvoiceSample.DataDrivenEntity.Tests
{
    public class InitializationContextTests
    {
        private class TestEntity : IDataDrivenEntityBase
        {
            public Guid Id { get; set; } = Guid.NewGuid();
            public bool IsInitialized { get; set; } = true;
            public bool IsNew { get; set; }

            public string Name { get; set; } = "Test";

            public IEnumerable<IDataDrivenEntityBase> GetAllEntities(bool includeSelf = false) => includeSelf ? new[] { this } : [];
            public object GetEntityData() => this;
            public object GetKey() => Id;
        }

        private class DerivedTestEntity : TestEntity
        {
            public string AdditionalProperty { get; set; } = "Derived";
        }

        private class TestEntityWithChildren : TestEntity, IDataDrivenEntity
        {
            public List<TestEntity> Children { get; } = new();

            public new IEnumerable<IDataDrivenEntityBase> GetAllEntities()
            {
                var result = new List<IDataDrivenEntityBase> { this };
                result.AddRange(Children);
                return result;
            }

            public void Initialize(object entityData, IInitializationContext? initializationContext, bool isNew = false)
            {
                // Simple implementation for testing
                IsInitialized = true;
                IsNew = isNew;

                initializationContext?.Add(this);

                foreach (var child in Children)
                {
                    initializationContext?.Add(child);
                }
            }
        }

        private class CircularReferenceEntity : TestEntity
        {
            public CircularReferenceEntity? Parent { get; set; }
            public List<CircularReferenceEntity> Children { get; } = new();

            public new IEnumerable<IDataDrivenEntityBase> GetAllEntities()
            {
                var result = new List<IDataDrivenEntityBase> { this };
                result.AddRange(Children);
                return result;
            }
        }

        [Fact]
        public void Add_ShouldStoreEntityInContext()
        {
            // Arrange
            var context = new InitializationContext();
            var entity = new TestEntity();

            // Act
            context.Add(entity);

            // Assert
            Assert.True(context.IsInitialized(entity));
        }

        [Fact]
        public void GetInitialized_WithExistingEntity_ShouldReturnEntity()
        {
            // Arrange
            var context = new InitializationContext();
            var entity = new TestEntity();
            context.Add(entity);

            // Act
            var result = context.GetInitialized((entity.GetType(), entity.GetKey()));

            // Assert
            Assert.Same(entity, result);
        }

        [Fact]
        public void GetInitialized_WithCompatibleDerivedType_ShouldReturnEntity()
        {
            // Arrange
            var context = new InitializationContext();
            var derivedEntity = new DerivedTestEntity { Id = Guid.NewGuid() };
            context.Add(derivedEntity);

            // Act
            var result = context.GetInitialized((typeof(TestEntity), derivedEntity.Id));

            // Assert
            Assert.Same(derivedEntity, result);
        }

        [Fact]
        public void FindByKey_ShouldFindEntityWithMatchingKey()
        {
            // Arrange
            var context = new InitializationContext();
            var id = Guid.NewGuid();
            var entity = new TestEntity { Id = id };
            context.Add(entity);

            // Act
            var result = context.FindByKey(id);

            // Assert
            Assert.Same(entity, result);
        }

        [Fact]
        public void Add_AlreadyAddedEntity_ShouldNotAddDuplicate()
        {
            // Arrange
            var context = new InitializationContext();
            var entity = new TestEntity();
            context.Add(entity);

            // Act
            context.Add(entity);  // Add same entity again

            // Assert
            Assert.Single(context.GetAllInitializedEntities());
        }

        [Fact]
        public void Add_DifferentEntitiesWithSameKey_ShouldAddBoth()
        {
            // Arrange
            var context = new InitializationContext();
            var id = Guid.NewGuid();
            var entity1 = new TestEntity { Id = id };
            var entity2 = new DerivedTestEntity { Id = id };

            // Act
            context.Add(entity1);
            context.Add(entity2);

            // Assert
            Assert.Equal(2, context.GetAllInitializedEntities().Count());
        }

        [Fact]
        public void Add_EntityWithChildren_ShouldTrackAllEntities()
        {
            // Arrange
            var context = new InitializationContext();
            var parent = new TestEntityWithChildren();
            parent.Children.Add(new TestEntity());
            parent.Children.Add(new TestEntity());

            // Act
            parent.Initialize(null, context);

            // Assert
            Assert.Equal(3, context.GetAllInitializedEntities().Count());
        }

        [Fact]
        public void EventSubscription_ShouldReceiveNotifications()
        {
            // Arrange
            var context = new InitializationContext();
            var entity = new TestEntity();
            var notificationReceived = false;

            // Act
            context.SubscribeToInitializationEvents(e => {
                if (e.Entity == entity)
                    notificationReceived = true;
            });

            context.Add(entity);

            // Assert
            Assert.True(notificationReceived);
        }

        [Fact]
        public void EventSubscription_ShouldReceivePastEvents()
        {
            // Arrange
            var context = new InitializationContext();
            var entity = new TestEntity();
            context.Add(entity);

            var notificationReceived = false;

            // Act - Subscribe after adding
            context.SubscribeToInitializationEvents(e => {
                if (e.Entity == entity)
                    notificationReceived = true;
            });

            // Assert
            Assert.True(notificationReceived);
        }

        [Fact]
        public void Unsubscribe_ShouldStopReceivingEvents()
        {
            // Arrange
            var context = new InitializationContext();
            var eventCount = 0;

            Action<EntityInitializedEvent> subscriber = _ => eventCount++;
            context.SubscribeToInitializationEvents(subscriber);

            // Add one entity, should trigger event
            context.Add(new TestEntity());
            Assert.Equal(1, eventCount);

            // Act - Unsubscribe
            context.UnsubscribeFromInitializationEvents(subscriber);

            // Add another entity, should not trigger for unsubscribed
            context.Add(new TestEntity());

            // Assert
            Assert.Equal(1, eventCount); // Still just 1
        }

        [Fact]
        public void CircularReferences_ShouldNotCauseStackOverflow()
        {
            // Arrange
            var context = new InitializationContext();
            var parent = new CircularReferenceEntity();
            var child = new CircularReferenceEntity { Parent = parent };
            parent.Children.Add(child);

            // Act - This should not cause stack overflow
            context.Add(parent);
            context.Add(child);

            // Assert
            Assert.Equal(2, context.GetAllInitializedEntities().Count());
        }
    }
}