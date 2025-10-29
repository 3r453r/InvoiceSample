using InvoiceSample.DataDrivenEntity.Implementations.Helpers;
using System;
using Xunit;

namespace InvoiceSample.DataDrivenEntity.Tests
{
    public class InitializationContextTestsBase
    {
        private class TestEntity : IDataDrivenEntityBase
        {
            public Guid Id { get; set; } = Guid.NewGuid();
            public bool IsInitialized { get; } = true;
            public bool IsNew { get; set; }

            public IEnumerable<IDataDrivenEntityBase> GetAllEntities(bool includeSelf = false) => includeSelf ?  new[] { this } : [];
            public object GetEntityData() => this;
            public object GetKey() => Id;
        }

        private class DerivedTestEntity : TestEntity
        {
            public string Name { get; set; } = "Derived";
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
        public void IsInitialized_WithExistingEntity_ShouldReturnTrue()
        {
            // Arrange
            var context = new InitializationContext();
            var entity = new TestEntity();
            context.Add(entity);

            // Act
            var result = context.IsInitialized(entity);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsInitialized_WithNonExistingEntity_ShouldReturnFalse()
        {
            // Arrange
            var context = new InitializationContext();
            var entity = new TestEntity();

            // Act
            var result = context.IsInitialized(entity);

            // Assert
            Assert.False(result);
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
        public void GetInitialized_WithNonExistingEntity_ShouldReturnNull()
        {
            // Arrange
            var context = new InitializationContext();
            var entity = new TestEntity();

            // Act
            var result = context.GetInitialized((entity.GetType(), entity.GetKey()));

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void Add_MultipleEntitiesWithSameKeyButDifferentTypes_ShouldStoreAllEntities()
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
            Assert.True(context.IsInitialized(entity1));
            Assert.True(context.IsInitialized(entity2));
            Assert.Same(entity1, context.GetInitialized((typeof(TestEntity), id)));
            Assert.Same(entity2, context.GetInitialized((typeof(DerivedTestEntity), id)));
        }
    }
}