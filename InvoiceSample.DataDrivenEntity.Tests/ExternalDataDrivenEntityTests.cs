using AutoMapper;
using InvoiceSample.DataDrivenEntity.Implementations;
using InvoiceSample.DataDrivenEntity.Tests.Data.TestEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace InvoiceSample.DataDrivenEntity.Tests
{
    public class ExternalDataDrivenEntityTests
    {
        private class TestExternalData
        {
            public required string ExternalValue { get; set; }
        }

        private interface ITestEntityData : IEntityData<Guid>
        {
            string Name { get; set; }
            IEnumerable<IChildEntityData> Children { get; }
        }

        private interface IChildEntityData : IEntityData<Guid>
        {
            string Value { get; set; }
        }

        private class ChildEntityData : IChildEntityData
        {
            public Guid Id { get; set; } = Guid.NewGuid();
            public string Value { get; set; } = "";
            public bool IsNew { get; set; }
            public Guid GetKey() => Id;
            object IEntityData.GetKey() => Id;
        }

        private class TestEntityData : ITestEntityData
        {
            public Guid Id { get; set; } = Guid.NewGuid();
            public string Name { get; set; } = "";
            public List<ChildEntityData> Children { get; set; } = [];
            public bool IsNew { get; set; }
            IEnumerable<IChildEntityData> ITestEntityData.Children => Children;

            public Guid GetKey() => Id;
            object IEntityData.GetKey() => Id;
        }

        private class ChildEntity : ExternalDataDrivenEntity<Guid, IChildEntityData, TestExternalData>, IChildEntityData
        {
            private bool _initialized = false;

            public Guid Id { get; set; }
            public string Value { get; set; } = "";

            protected override bool SelfInitialzed => _initialized;

            public override IChildEntityData GetEntityData() => this;

            public override Guid GetKey() => Id;

            protected override void SelfInitialize(IChildEntityData entityData, TestExternalData externalData)
            {
                Id = entityData.GetKey();
                Value = entityData.Value + externalData.ExternalValue;
                _initialized = true;
            }

            object IEntityData.GetKey() => Id;
        }

        private class TestEntity : ExternalDataDrivenEntity<Guid, ITestEntityData, TestExternalData>, ITestEntityData
        {
            private bool _initialized = false;

            public TestEntity()
            {
                RegisterExternalChildCollection<ChildEntity, Guid, IChildEntityData, TestExternalData>(
                    Children,
                    d => d.Children,
                    (_, _) => new ChildEntity(),
                    _ => new TestExternalData { ExternalValue = "_external" }
                );
            }

            public Guid Id { get; set; }
            public string Name { get; set; } = "";
            public List<ChildEntity> Children { get; } = [];

            protected override bool SelfInitialzed => _initialized;

            IEnumerable<IChildEntityData> ITestEntityData.Children => Children;

            public override ITestEntityData GetEntityData() => this;

            public override Guid GetKey() => Id;

            protected override void SelfInitialize(ITestEntityData entityData, TestExternalData externalData)
            {
                Id = entityData.GetKey();
                Name = entityData.Name + externalData.ExternalValue;
                _initialized = true;
            }

            object IEntityData.GetKey() => Id;
        }

        [Fact]
        public void Initialize_WithExternalData_ShouldProperlyInitializeEntity()
        {
            // Arrange
            var entityData = new TestEntityData
            {
                Id = Guid.NewGuid(),
                Name = "Test"
            };
            var externalData = new TestExternalData
            {
                ExternalValue = "_external"
            };
            var entity = new TestEntity();

            // Act
            entity.Initialize(entityData, externalData);

            // Assert
            Assert.True(entity.IsInitialized);
            Assert.Equal(entityData.Id, entity.Id);
            Assert.Equal("Test_external", entity.Name);
        }

        [Fact]
        public void Initialize_WithChildren_ShouldInitializeChildEntities()
        {
            // Arrange
            var childData = new ChildEntityData
            {
                Id = Guid.NewGuid(),
                Value = "Child"
            };
            var entityData = new TestEntityData
            {
                Id = Guid.NewGuid(),
                Name = "Parent",
                Children = [childData]
            };
            var externalData = new TestExternalData
            {
                ExternalValue = "_external"
            };
            var entity = new TestEntity();

            // Act
            entity.Initialize(entityData, externalData);

            // Assert
            Assert.Single(entity.Children);
            Assert.Equal(childData.Id, entity.Children[0].Id);
            Assert.Equal("Child_external", entity.Children[0].Value);
            Assert.True(entity.Children[0].IsInitialized);
        }

        [Fact]
        public void GetAllEntities_ShouldReturnAllChildEntities()
        {
            // Arrange
            var entityData = new TestEntityData
            {
                Id = Guid.NewGuid(),
                Name = "Parent",
                Children = [
                    new ChildEntityData { Id = Guid.NewGuid(), Value = "Child1" },
                    new ChildEntityData { Id = Guid.NewGuid(), Value = "Child2" }
                ]
            };
            var externalData = new TestExternalData
            {
                ExternalValue = "_external"
            };
            var entity = new TestEntity();

            // Act
            entity.Initialize(entityData, externalData);
            var allEntities = entity.GetAllEntities(true).ToList();

            // Assert
            // We should have 3 entities: parent and 2 children
            Assert.Equal(3, allEntities.Count);

            // Verify parent and children are included in the collection
            Assert.Contains(entity, allEntities);
            Assert.Equal(2, allEntities.Count(e => e != entity));
        }
    }
}