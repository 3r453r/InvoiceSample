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
using System.Text.Json;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity.Tests
{
    public class DataDrivenEntityTests
    {
        [Theory]
        [ClassData(typeof(TestDataProvider))]
        public void Initialize_Should_Properly_Import_Data(TestData testData)
        {
            //Arrange
            var originalInvoice = new Invoice();
            originalInvoice.Initialize(testData.OriginalData);

            var modifiedInvoice = new Invoice();
            modifiedInvoice.Initialize(testData.ModifiedData);
            //Act
            testData.Modify(originalInvoice);

            //Assert
            originalInvoice.GetEntityData().Should().BeEquivalentTo(modifiedInvoice.GetEntityData());
        }

        [Theory]
        [ClassData(typeof(TestDataProvider))]
        public void Initialize_Should_Properly_Update_Data(TestData testData)
        {
            //Arrange
            var originalInvoice = new Invoice();
            originalInvoice.Initialize(testData.OriginalData);

            //Act
            originalInvoice.Initialize(testData.ModifiedData);

            //Assert
            originalInvoice.GetEntityData().Should().BeEquivalentTo((IInvoiceData)testData.ModifiedData);
        }
    }
}
