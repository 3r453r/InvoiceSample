using InvoiceSample.DataDrivenEntity.Implementations.Basic;
using InvoiceSample.DataDrivenEntity.Implementations.Helpers;
using System;
using System.Collections.Generic;
using Xunit;

namespace InvoiceSample.DataDrivenEntity.Tests
{
    public class NullPropertyTest
    {
        [Fact]
        public void Initialize_WithNullProperty_ShouldSetPropertyToNull()
        {
            // Arrange
            var entityData = new TestEntityData
            {
                Id = Guid.NewGuid(),
                Property1 = "Initial value",
                Property2 = "Will be null"
            };

            var entity = new TestEntity();

            // Act - First initialize with non-null values
            entity.Initialize(entityData);

            // Verify initial state
            Assert.Equal("Initial value", entity.Property1);
            Assert.Equal("Will be null", entity.Property2);

            // Now set Property2 to null in the data
            entityData.Property2 = null;

            // Act - Reinitialize with Property2 as null
            entity.Initialize(entityData);

            // Assert
            Assert.Equal("Initial value", entity.Property1);
            Assert.Null(entity.Property2);
        }

        private class TestEntityData : IEntityData<Guid>
        {
            public Guid Id { get; set; }
            public string Property1 { get; set; }
            public string Property2 { get; set; }
            public bool IsNew { get; set; }
            public Guid GetKey() => Id;
            object IEntityData.GetKey() => Id;
        }

        private class TestEntity : DataDrivenEntity<Guid, TestEntityData>
        {
            private bool _initialized = false;

            public Guid Id { get; private set; }
            public string Property1 { get; private set; }
            public string Property2 { get; private set; }

            protected override bool SelfInitialzed => _initialized;

            public override TestEntityData GetEntityData()
            {
                return new TestEntityData
                {
                    Id = Id,
                    Property1 = Property1,
                    Property2 = Property2
                };
            }

            public override Guid GetKey() => Id;

            protected override void SelfInitialize(TestEntityData entityData)
            {
                Id = entityData.Id;
                Property1 = entityData.Property1;
                Property2 = entityData.Property2;
                _initialized = true;
            }
        }
    }
}