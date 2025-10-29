using InvoiceSample.DataDrivenEntity.Implementations.Helpers;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace InvoiceSample.DataDrivenEntity.Tests
{
    public class CollectionWrapperTests
    {
        private class TestEntity : IDataDrivenEntity
        {
            public Guid Id { get; set; } = Guid.NewGuid();
            public bool IsInitialized { get; } = true;
            public bool IsNew { get; set; }

            public IEnumerable<IDataDrivenEntityBase> GetAllEntities(bool includeSelf = false) => new[] { this };
            public object GetEntityData() => this;
            public object GetKey() => Id;
            public void Initialize(object entityData, IInitializationContext? initializationContext, bool isNew = false) { }
        }

        private class ExternalTestEntity : IExternalDataDrivenEntity
        {
            public Guid Id { get; set; } = Guid.NewGuid();
            public bool IsInitialized { get; } = true;
            public bool IsNew { get; set; }

            public IEnumerable<IDataDrivenEntityBase> GetAllEntities(bool includeSelf = false) => new[] { this };
            public object GetEntityData() => this;
            public object GetKey() => Id;
            public void Initialize(object entityData, object externalData, IInitializationContext? initializationContext, bool isNew = false) { }
        }

        [Fact]
        public void CollectionWrapper_Add_ShouldAddToInnerCollection()
        {
            // Arrange
            var innerCollection = new List<TestEntity>();
            var wrapper = new CollectionWrapper<TestEntity>(innerCollection);
            var entity = new TestEntity();

            // Act
            wrapper.Add(entity);

            // Assert
            Assert.Single(innerCollection);
            Assert.Same(entity, innerCollection[0]);
        }

        [Fact]
        public void CollectionWrapper_Add_WithWrongType_ShouldThrowException()
        {
            // Arrange
            var innerCollection = new List<TestEntity>();
            var wrapper = new CollectionWrapper<TestEntity>(innerCollection);

            // Create a mock IDataDrivenEntity that's not a TestEntity
            var wrongTypeEntity = new Mock<IDataDrivenEntity>().Object;

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => wrapper.Add(wrongTypeEntity));
        }

        [Fact]
        public void CollectionWrapper_Remove_ShouldRemoveFromInnerCollection()
        {
            // Arrange
            var entity = new TestEntity();
            var innerCollection = new List<TestEntity> { entity };
            var wrapper = new CollectionWrapper<TestEntity>(innerCollection);

            // Act
            bool result = wrapper.Remove(entity);

            // Assert
            Assert.True(result);
            Assert.Empty(innerCollection);
        }

        [Fact]
        public void CollectionWrapper_Remove_WithWrongType_ShouldReturnFalse()
        {
            // Arrange
            var innerCollection = new List<TestEntity> { new TestEntity() };
            var wrapper = new CollectionWrapper<TestEntity>(innerCollection);

            // Create a mock IDataDrivenEntity that's not a TestEntity
            var wrongTypeEntity = new Mock<IDataDrivenEntity>().Object;

            // Act
            bool result = wrapper.Remove(wrongTypeEntity);

            // Assert
            Assert.False(result);
            Assert.Single(innerCollection);
        }

        [Fact]
        public void CollectionWrapper_GetEnumerator_ShouldReturnAllItems()
        {
            // Arrange
            var entity1 = new TestEntity();
            var entity2 = new TestEntity();
            var innerCollection = new List<TestEntity> { entity1, entity2 };
            var wrapper = new CollectionWrapper<TestEntity>(innerCollection);

            // Act
            var result = wrapper.ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains(entity1, result);
            Assert.Contains(entity2, result);
        }

        [Fact]
        public void ExternalCollectionWrapper_Add_ShouldAddToInnerCollection()
        {
            // Arrange
            var innerCollection = new List<ExternalTestEntity>();
            var wrapper = new ExternalCollectionWrapper<ExternalTestEntity>(innerCollection);
            var entity = new ExternalTestEntity();

            // Act
            wrapper.Add(entity);

            // Assert
            Assert.Single(innerCollection);
            Assert.Same(entity, innerCollection[0]);
        }

        [Fact]
        public void ExternalCollectionWrapper_Remove_ShouldRemoveFromInnerCollection()
        {
            // Arrange
            var entity = new ExternalTestEntity();
            var innerCollection = new List<ExternalTestEntity> { entity };
            var wrapper = new ExternalCollectionWrapper<ExternalTestEntity>(innerCollection);

            // Act
            bool result = wrapper.Remove(entity);

            // Assert
            Assert.True(result);
            Assert.Empty(innerCollection);
        }
    }
}