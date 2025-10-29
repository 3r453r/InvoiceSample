using AutoMapper;
using InvoiceSample.Application.EventBus;
using InvoiceSample.Application.Events.Integration;
using InvoiceSample.Application.Persistence;
using InvoiceSample.Application.Services.Invoice;
using InvoiceSample.Domain;
using InvoiceSample.Domain.Exceptions;
using InvoiceSample.Domain.InvoiceAggregate;
using InvoiceSample.Domain.SalesOrderAggregate;
using InvoiceSample.Domain.WarehouseReleaseAggregate;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace InvoiceSample.DataDrivenEntity.Tests
{
    public class InvoiceServiceTests
    {
        [Fact]
        public async Task EndPeriod_WithPeriodicInvoice_ShouldEndPeriodAndComplete()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IInvoiceSampleUnitOfWork>();
            var eventBusMock = new Mock<IEventBus>();
            var mapperMock = new Mock<IMapper>();
            var invoiceRepositoryMock = new Mock<IInvoiceRepository>();

            unitOfWorkMock.Setup(u => u.InvoiceRepository).Returns(invoiceRepositoryMock.Object);

            var customerId = Guid.NewGuid();

            // Create a mock sales order
            var salesOrderMock = new Mock<ISalesOrderData>();
            salesOrderMock.Setup(s => s.Number).Returns("SO-001");

            // Create a test invoice data with the sales order in its SalesOrders collection
            var testInvoiceData = new TestInvoiceData
            {
                Type = InvoiceType.Periodic,
                SalesOrdersList = new List<ISalesOrderData> { salesOrderMock.Object }
            };

            invoiceRepositoryMock.Setup(r => r.GetPeriodicDraftByCustomer(customerId))
                .ReturnsAsync(testInvoiceData);

            // Instead of mocking PeriodicInvoice, create a custom testable implementation
            var testPeriodicInvoice = new TestablePeriodicInvoice(salesOrderMock.Object, mapperMock.Object);

            // Create a partial mock of InvoiceService where we override the factory method
            var invoiceServiceMock = new Mock<InvoiceService>(unitOfWorkMock.Object, eventBusMock.Object, mapperMock.Object)
            {
                CallBase = true
            };

            // Override the CreatePeriodicInvoice method to return our testable implementation
            invoiceServiceMock.Setup(s => s.CreatePeriodicInvoice(It.IsAny<ISalesOrderData>(), It.IsAny<IMapper>()))
                .Returns(testPeriodicInvoice);

            // Add callback to ensure completion happens during Update
            bool updateWasCalled = false;
            invoiceRepositoryMock.Setup(r => r.Update(It.IsAny<IInvoiceData>()))
                .Callback<IInvoiceData>(invoice => {
                    updateWasCalled = true;
                    // Verify this is our test implementation
                    if (invoice is TestablePeriodicInvoice testInvoice)
                    {
                        // At this point, EndPeriod and Complete should have been called
                        Assert.True(testInvoice.EndPeriodWasCalled);
                        Assert.True(testInvoice.CompleteWasCalled);
                    }
                })
                .Returns(Task.CompletedTask);

            // Act
            var result = await invoiceServiceMock.Object.EndPeriod(customerId);

            // Assert
            Assert.True(updateWasCalled, "Repository.Update was not called");
            Assert.True(testPeriodicInvoice.EndPeriodWasCalled, "EndPeriod was not called");
            Assert.True(testPeriodicInvoice.IsReadyToCompleteWasCalled, "IsReadyToComplete was not called");
            Assert.True(testPeriodicInvoice.CompleteWasCalled, "Complete was not called");
            eventBusMock.Verify(e => e.PublishIntegrationEvent(It.IsAny<PrintInvoiceRequested>()), Times.Once);
        }

        [Fact]
        public async Task UpdateInvoice_WithWarehouseReleaseData_ShouldUpdateInvoiceLines()
        {
            // Arrange
            var unitOfWorkMock = new Mock<IInvoiceSampleUnitOfWork>();
            var eventBusMock = new Mock<IEventBus>();
            var mapperMock = new Mock<IMapper>();
            var invoiceRepositoryMock = new Mock<IInvoiceRepository>();

            unitOfWorkMock.Setup(u => u.InvoiceRepository).Returns(invoiceRepositoryMock.Object);

            // Create a warehouse release data mock
            var warehouseReleaseMock = new Mock<IWarehouseReleaseData>();
            warehouseReleaseMock.Setup(w => w.SalesOrderNumber).Returns("SO-001");
            warehouseReleaseMock.Setup(w => w.Lines).Returns(new List<IWarehouseReleaseLine>());

            // Create a mock sales order
            var salesOrderMock = new Mock<ISalesOrderData>();
            salesOrderMock.Setup(s => s.Number).Returns("SO-001");

            // Create a test invoice data with the sales order in its SalesOrders collection
            var testInvoiceData = new TestInvoiceData
            {
                Type = InvoiceType.Automatic,
                SalesOrdersList = new List<ISalesOrderData> { salesOrderMock.Object }
            };

            // Set up repository
            invoiceRepositoryMock.Setup(r => r.GetDraftBySalesOrderNumber("SO-001"))
                .ReturnsAsync(testInvoiceData);
            invoiceRepositoryMock.Setup(r => r.SalesOrderInvoiced("SO-001"))
                .ReturnsAsync(false);

            // Create a testable implementation
            var testAutomaticInvoice = new TestableAutomaticInvoice(salesOrderMock.Object, mapperMock.Object);

            // Create a partial mock of InvoiceService
            var invoiceServiceMock = new Mock<InvoiceService>(unitOfWorkMock.Object, eventBusMock.Object, mapperMock.Object)
            {
                CallBase = true
            };

            // Override the CreateAutomaticInvoice method
            invoiceServiceMock.Setup(s => s.CreateAutomaticInvoice(It.IsAny<ISalesOrderData>(), It.IsAny<IMapper>()))
                .Returns(testAutomaticInvoice);

            // Add callback to ensure methods are called during Update
            bool updateWasCalled = false;
            invoiceRepositoryMock.Setup(r => r.Update(It.IsAny<IInvoiceData>()))
                .Callback<IInvoiceData>(invoice => {
                    updateWasCalled = true;
                    // Verify this is our test implementation
                    if (invoice is TestableAutomaticInvoice testInvoice)
                    {
                        // At this point, UpdateLines should have been called
                        Assert.True(testInvoice.UpdateLinesWasCalled);
                    }
                })
                .Returns(Task.CompletedTask);

            // Act
            var result = await invoiceServiceMock.Object.UpdateInvoice(warehouseReleaseMock.Object);

            // Assert
            Assert.True(updateWasCalled, "Repository.Update was not called");
            Assert.True(testAutomaticInvoice.UpdateLinesWasCalled, "UpdateLines was not called");
        }

        // Custom testable implementations instead of mocks
        private class TestablePeriodicInvoice : PeriodicInvoice
        {
            public bool EndPeriodWasCalled { get; private set; }
            public bool IsReadyToCompleteWasCalled { get; private set; }
            public bool CompleteWasCalled { get; private set; }

            public TestablePeriodicInvoice(ISalesOrderData salesOrderData, IMapper mapper)
                : base(salesOrderData, mapper)
            {
            }

            public override void EndPeriod()
            {
                EndPeriodWasCalled = true;
                base.EndPeriod();
            }

            public override bool IsReadyToComplete()
            {
                IsReadyToCompleteWasCalled = true;
                return true; // Always ready for testing
            }

            public override void Complete()
            {
                CompleteWasCalled = true;
                base.Complete();
            }
        }

        private class TestableAutomaticInvoice : AutomaticInvoice
        {
            public bool UpdateLinesWasCalled { get; private set; }
            public bool IsReadyToCompleteWasCalled { get; private set; }

            public TestableAutomaticInvoice(ISalesOrderData salesOrderData, IMapper mapper)
                : base(salesOrderData, mapper)
            {
            }

            public override void UpdateLines(IWarehouseReleaseData warehouseReleaseData)
            {
                UpdateLinesWasCalled = true;
                // Don't call base to avoid real implementation that could fail in tests
            }

            public override bool IsReadyToComplete()
            {
                IsReadyToCompleteWasCalled = true;
                return false; // Not ready for testing
            }
        }

        // Test data class with explicit SalesOrders implementation
        private class TestInvoiceData : IInvoiceData
        {
            public string Number => "I/TEST-001";
            public string SalesOrderNumber => "SO-001";
            public Guid Id => Guid.NewGuid();
            public Guid CustomerId => Guid.NewGuid();
            public InvoiceType Type { get; set; } = InvoiceType.Automatic;
            public InvoiceState State => InvoiceState.Draft;
            public decimal NetValue => 0m;
            public decimal VatValue => 0m;
            public decimal GrossValue => 0m;
            public DateTime? InvoiceDate => null;
            public int? PaymentTermInDays => 14;
            public DateTime? DueDate => null;
            public IEnumerable<IVatSum> VatSums { get; set; } = new List<IVatSum>();
            public bool IsNew { get; set; }
            // Important: allow setting of SalesOrders for test
            public List<ISalesOrderData> SalesOrdersList { get; set; } = new List<ISalesOrderData>();
            public IEnumerable<ISalesOrderData> SalesOrders => SalesOrdersList;

            public IEnumerable<IInvoiceLine> Lines => new List<IInvoiceLine>();
            IEnumerable<IDocumentLine> IDocument.Lines => Lines;

            public string GetKey() => Number;
            object IEntityData.GetKey() => Number;
            Guid IEntityData<Guid>.GetKey() => Id;
        }
    }
}