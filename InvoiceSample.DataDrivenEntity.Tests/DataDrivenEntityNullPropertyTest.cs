using InvoiceSample.DataDrivenEntity.Implementations.Basic;
using InvoiceSample.DataDrivenEntity.Implementations.Helpers;
using System;
using System.Collections.Generic;
using Xunit;

namespace InvoiceSample.DataDrivenEntity.Tests
{
    public class DataDrivenEntityNullPropertyTest
    {
        [Fact]
        public void Initialize_WithNullChildEntity_ShouldSetPropertyToNull()
        {
            // Arrange
            var context = new InitializationContext();

            // Create parent entity data with a child
            var childData = new TestChildData { Id = Guid.NewGuid(), Name = "Child" };
            var parentData = new TestParentData
            {
                Id = Guid.NewGuid(),
                Name = "Parent",
                Child = childData
            };

            // Create and initialize parent entity with child
            var parentEntity = new TestParentEntity();
            parentEntity.Initialize(parentData);

            // Verify initial state
            Assert.NotNull(parentEntity.Child);
            Assert.Equal("Child", parentEntity.Child.Name);

            // Now set the child to null in the parent data
            parentData.Child = null;

            // Act - Reinitialize parent with null child
            parentEntity.Initialize(parentData);

            // Assert - Child should be null
            Assert.Null(parentEntity.Child);
        }

        // Test entity classes
        private class TestChildData : IEntityData<Guid>
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = "";
            public bool IsNew { get; set; }

            public Guid GetKey() => Id;
            object IEntityData.GetKey() => Id;
        }

        private class TestChildEntity : DataDrivenEntity<Guid, TestChildData>
        {
            private bool _initialized = false;

            public Guid Id { get; private set; }
            public string Name { get; private set; }

            protected override bool SelfInitialzed => _initialized;

            public override TestChildData GetEntityData()
            {
                return new TestChildData
                {
                    Id = Id,
                    Name = Name
                };
            }

            public override Guid GetKey() => Id;

            protected override void SelfInitialize(TestChildData entityData)
            {
                Id = entityData.Id;
                Name = entityData.Name;
                _initialized = true;
            }
        }

        private class TestParentData : IEntityData<Guid>
        {
            public Guid Id { get; set; }
            public string Name { get; set; } = "";
            public TestChildData? Child { get; set; }
            public bool IsNew { get; set; }

            public Guid GetKey() => Id;
            object IEntityData.GetKey() => Id;
        }

        private class TestParentEntity : DataDrivenEntity<Guid, TestParentData>
        {
            private bool _initialized = false;

            // Important: Make Child publicly settable for the test
            public Guid Id { get; private set; }
            public string Name { get; private set; }
            public TestChildEntity Child { get; set; }

            public TestParentEntity()
            {
                RegisterChild<Guid, TestChildData>(
                    Child,
                    data => data.Child,
                    entity => { Child = null; }, // This is critical: must set Child to null
                    entity => { Child = (TestChildEntity)entity; },
                    data => new TestChildEntity()
                );
            }

            protected override bool SelfInitialzed => _initialized;

            public override TestParentData GetEntityData()
            {
                return new TestParentData
                {
                    Id = Id,
                    Name = Name,
                    Child = Child?.GetEntityData()
                };
            }

            public override Guid GetKey() => Id;

            protected override void SelfInitialize(TestParentData entityData)
            {
                Id = entityData.Id;
                Name = entityData.Name;
                _initialized = true;
            }
        }
    }
}