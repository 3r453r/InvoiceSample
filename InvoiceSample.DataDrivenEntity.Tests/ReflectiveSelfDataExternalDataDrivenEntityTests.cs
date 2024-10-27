using FluentAssertions;
using InvoiceSample.DataDrivenEntity.Aggregates;
using InvoiceSample.DataDrivenEntity.Tests.Data;
using InvoiceSample.DataDrivenEntity.Tests.Data.InvoiceDomain;
using InvoiceSample.DataDrivenEntity.Tests.Data.InvoiceDomain.Data;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity.Tests
{
    public class ReflectiveSelfDataExternalDataDrivenEntityTests
    {
        [Theory]
        [ClassData(typeof(TestDataProvider))]
        public void Initialize_Should_ProperlyImportData(TestData testData)
        {
            //Arrange
            var invoice = new Invoice();
            invoice.Initialize(testData.OriginalData);

            //Act
            testData.Modify(invoice);

            //Assert

            testData.ModifiedData.Should().Be(invoice);
        }

        //[Fact]
        //public void ChildEntries_Should_Contain_Child_From_Reflection()
        //{
        //    // Arrange
        //    var entity = new TestEntity();
        //    entity.Child = new TestChildEntity();

        //    // Act
        //    var childEntries = entity.ChildEntries;

        //    // Assert
        //    childEntries.Should().ContainSingle();
        //    var childEntry = childEntries.First();
        //    childEntry.Selector.Should().Be("Child");
        //    childEntry.Entity.Should().Be(entity.Child);
        //}

        //[Fact]
        //public void CollectionEntries_Should_Contain_ChildEntities_From_Reflection()
        //{
        //    // Arrange
        //    var entity = new TestEntity();
        //    entity.ChildEntities.Add(new TestChildEntity());

        //    // Act
        //    var collectionEntries = entity.CollectionEntries;

        //    // Assert
        //    collectionEntries.Should().ContainSingle();
        //    var collectionEntry = collectionEntries.First();
        //    collectionEntry.CollectionSelector.Should().Be("ChildEntities");
        //    collectionEntry.Collection.Should().BeEquivalentTo(entity.ChildEntities);
        //}

        //[Fact]
        //public void CreateChild_Should_Create_New_ChildEntity()
        //{
        //    // Arrange
        //    var entity = new TestEntity();

        //    // Act
        //    var child = entity.CreateChild("Child");

        //    // Assert
        //    child.Should().NotBeNull();
        //    child.Should().BeOfType<TestChildEntity>();
        //}

        //[Fact]
        //public void CreateChild_With_Invalid_Selector_Should_Throw()
        //{
        //    // Arrange
        //    var entity = new TestEntity();

        //    // Act
        //    Action act = () => entity.CreateChild("InvalidSelector");

        //    // Assert
        //    act.Should().Throw<InvalidOperationException>().WithMessage("No child creator found for selector: InvalidSelector");
        //}
    }
}
