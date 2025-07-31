using InvoiceSample.Application.EventBus;
using InvoiceSample.Application.Events.Integration;
using InvoiceSample.Application.Persistence;
using InvoiceSample.Domain.Exceptions;
using InvoiceSample.Domain.InvoiceAggregate;
using InvoiceSample.Domain.SalesOrderAggregate;
using InvoiceSample.Domain.WarehouseReleaseAggregate;
using System;
using System.Threading.Tasks;

namespace InvoiceSample.Application.Services.Invoice
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceSampleUnitOfWork _unitOfWork;
        private readonly IEventBus _eventBus;
        private readonly AutoMapper.IMapper _mapper;

        public InvoiceService(IInvoiceSampleUnitOfWork unitOfWork, IEventBus eventBus, AutoMapper.IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _eventBus = eventBus;
            _mapper = mapper;
        }

        public async Task<IInvoiceData?> GetInvoice(string invoiceNumber)
        {
            return await _unitOfWork.InvoiceRepository.GetByNumber(invoiceNumber);
        }

        public async Task<IInvoiceData> AddOrUpdateInvoice(ISalesOrderData salesOrderData)
        {
            var alreadyInvoiced = await _unitOfWork.InvoiceRepository.SalesOrderInvoiced(salesOrderData.Number);
            if (alreadyInvoiced)
            {
                throw new BusinessRuleException($"salesOrder {salesOrderData.Number} already invoiced");
            }

            var invoiceData = await _unitOfWork.InvoiceRepository.GetDraftBySalesOrderNumber(salesOrderData.Number);
            if (invoiceData is not null)
            {
                var invoice = invoiceData.Type == InvoiceType.Automatic
                    ? CreateAutomaticInvoice(salesOrderData, _mapper)
                    : CreatePeriodicInvoice(salesOrderData, _mapper) as Domain.InvoiceAggregate.Invoice;
                invoice.Initialize(invoiceData, _mapper);
                await _unitOfWork.InvoiceRepository.Update(invoice);
                return invoice;
            }
            else
            {
                // Create a new invoice
                var invoice = salesOrderData.AutoInvoice
                    ? CreateAutomaticInvoice(salesOrderData, _mapper)
                    : CreatePeriodicInvoice(salesOrderData, _mapper) as Domain.InvoiceAggregate.Invoice;
                await _unitOfWork.InvoiceRepository.Add(invoice);
                return invoice;
            }
        }

        public async Task<IInvoiceData> UpdateInvoice(IWarehouseReleaseData warehouseReleaseData)
        {
            var invoiceData = await _unitOfWork.InvoiceRepository.GetDraftBySalesOrderNumber(warehouseReleaseData.SalesOrderNumber);
            var alreadyInvoiced = await _unitOfWork.InvoiceRepository.SalesOrderInvoiced(warehouseReleaseData.SalesOrderNumber);
            if (alreadyInvoiced) { throw new BusinessRuleException($"salesOrder {warehouseReleaseData.SalesOrderNumber} already invoiced"); }
            if (invoiceData is null) { throw new BusinessRuleException($"invalid salesOrderNumber - {warehouseReleaseData.SalesOrderNumber}"); }

            // We need to get the sales order first
            var salesOrder = invoiceData.SalesOrders.FirstOrDefault(s => s.Number == warehouseReleaseData.SalesOrderNumber);
            if (salesOrder == null)
            {
                throw new BusinessRuleException($"invoice is not related to salesOrder {warehouseReleaseData.SalesOrderNumber}");
            }

            var invoice = invoiceData.Type == InvoiceType.Automatic
                ? CreateAutomaticInvoice(salesOrder, _mapper)
                : CreatePeriodicInvoice(salesOrder, _mapper) as Domain.InvoiceAggregate.Invoice;

            invoice.Initialize(invoiceData, _mapper);
            invoice.UpdateLines(warehouseReleaseData);
            if (invoice.IsReadyToComplete())
            {
                invoice.Complete();
                await _eventBus.PublishIntegrationEvent(new PrintInvoiceRequested
                {
                    Invoice = invoice,
                });
            }
            await _unitOfWork.InvoiceRepository.Update(invoice);
            return invoice;
        }

        public async Task<IInvoiceData?> EndPeriod(Guid customerId)
        {
            var invoiceData = await _unitOfWork.InvoiceRepository.GetPeriodicDraftByCustomer(customerId);
            if (invoiceData == null) { return null; }

            // Get the sales order from the invoice data
            var salesOrder = invoiceData.SalesOrders.FirstOrDefault();
            if (salesOrder == null)
            {
                throw new BusinessRuleException("No sales order found for the invoice");
            }

            var invoice = CreatePeriodicInvoice(salesOrder, _mapper);
            invoice.Initialize(invoiceData, _mapper);
            invoice.EndPeriod();
            if (invoice.IsReadyToComplete())
            {
                invoice.Complete();
                await _eventBus.PublishIntegrationEvent(new PrintInvoiceRequested
                {
                    Invoice = invoice,
                });
            }
            await _unitOfWork.InvoiceRepository.Update(invoice);
            return invoice;
        }

        // These virtual methods allow us to override them in tests
        public virtual PeriodicInvoice CreatePeriodicInvoice(ISalesOrderData salesOrderData, AutoMapper.IMapper mapper)
        {
            return new PeriodicInvoice(salesOrderData, mapper);
        }

        public virtual AutomaticInvoice CreateAutomaticInvoice(ISalesOrderData salesOrderData, AutoMapper.IMapper mapper)
        {
            return new AutomaticInvoice(salesOrderData, mapper);
        }

        public async Task SaveChanges()
        {
            await _unitOfWork.SaveChanges();
        }
    }
}