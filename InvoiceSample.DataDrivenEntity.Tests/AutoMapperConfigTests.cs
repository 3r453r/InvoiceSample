using AutoMapper;
using InvoiceSample.DataDrivenEntity.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace InvoiceSample.DataDrivenEntity.Tests
{
    public class AutoMapperConfigTests
    {
        private interface ITestEntity : IDataDrivenEntityBase
        {
            string Name { get; set; }
        }

        private class TestEntity : ITestEntity
        {
            public string Name { get; set; } = "";
            public bool IsInitialized { get; } = true;
            public IDataDrivenEntityBase ChildEntity { get; set; }
            public List<IDataDrivenEntityBase> ChildEntities { get; set; } = new();
            public IEnumerable<IDataDrivenEntityBase> GetAllEntities(bool includeSelf = false) => new[] { this };
            public object GetEntityData() => this;
            public object GetKey() => Name;
            public bool IsNew { get; set; }
        }

        private class TestEntityDto
        {
            public string Name { get; set; } = "";
            public IDataDrivenEntityBase ChildEntity { get; set; }
            public List<IDataDrivenEntityBase> ChildEntities { get; set; } = new();
        }

        [Fact]
        public void GloballyIgnoreProperties_ShouldIgnoreSpecifiedTypes()
        {
            // Arrange
            var config = new MapperConfiguration(cfg =>
            {
                cfg.GloballyIgnoreProperties(c => c
                    .Ignore<IDataDrivenEntityBase>()
                    .IgnoreCollections<IDataDrivenEntityBase>());

                cfg.CreateMap<TestEntityDto, TestEntity>();
            });

            // Act
            var mapper = config.CreateMapper();
            var dto = new TestEntityDto
            {
                Name = "Test",
                ChildEntity = new TestEntity { Name = "Child" },
                ChildEntities = new List<IDataDrivenEntityBase> { new TestEntity { Name = "ListChild" } }
            };

            var entity = mapper.Map<TestEntity>(dto);

            // Assert
            Assert.Equal("Test", entity.Name);
            Assert.Null(entity.ChildEntity); // Should be ignored
            Assert.Empty(entity.ChildEntities); // Should be ignored
        }

        [Fact]
        public void IgnoreBuilder_ShouldProduceCorrectFilter()
        {
            // Arrange
            var builder = new IgnoreBuilder();
            builder.Ignore<IDataDrivenEntityBase>()
                   .IgnoreCollections<IDataDrivenEntityBase>();

            // Act
            var filter = builder.Build();

            // Create test properties using reflection
            var entityProperty = typeof(TestEntity).GetProperty("ChildEntity");
            var entityCollectionProperty = typeof(TestEntity).GetProperty("ChildEntities");
            var stringProperty = typeof(TestEntity).GetProperty("Name");

            // Assert - filter returns false for properties that should not be mapped
            // and false for properties that should be ignored
            Assert.False(filter(entityProperty)); // Should be ignored
            Assert.False(filter(entityCollectionProperty)); // Should be ignored
            Assert.True(filter(stringProperty)); // Should be mapped
        }

        [Fact]
        public void IgnoreBuilder_WithMultipleIgnores_ShouldIgnoreAll()
        {
            // Arrange
            var builder = new IgnoreBuilder();
            builder.Ignore<IDataDrivenEntityBase>()
                   .Ignore<string>();

            // Act
            var filter = builder.Build();

            // Create test properties using reflection
            var entityProperty = typeof(TestEntity).GetProperty("ChildEntity");
            var stringProperty = typeof(TestEntity).GetProperty("Name");

            // Assert
            Assert.False(filter(entityProperty)); // Should be ignored
            Assert.False(filter(stringProperty)); // Should be ignored
        }
    }
}