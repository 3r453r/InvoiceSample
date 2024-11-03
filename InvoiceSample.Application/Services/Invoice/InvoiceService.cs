﻿using AutoMapper;
using InvoiceSample.Application.EventBus;
using InvoiceSample.Application.Events.Integration;
using InvoiceSample.Application.Persistence;
using InvoiceSample.Domain.Exceptions;
using InvoiceSample.Domain.InvoiceAggregate;
using InvoiceSample.Domain.SalesOrderAggregate;
using InvoiceSample.Domain.WarehouseReleaseAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Application.Services.Invoice
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceSampleUnitOfWork _unitOfWork;
        private readonly IEventBus _eventBus;
        private readonly IMapper _mapper;

        public InvoiceService(IInvoiceSampleUnitOfWork unitOfWork, IEventBus eventBus, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _eventBus = eventBus;
            _mapper = mapper;
        }

        public async Task<IInvoiceData> AddOrUpdateInvoice(ISalesOrderData salesOrderData)
        {
            if (await _unitOfWork.InvoiceRepository.SalesOrderInvoiced(salesOrderData.Number))
            {
                throw new BusinessRuleException("SalesOrder already invoiced");
            }

            var invoiceData = await _unitOfWork.InvoiceRepository.GetDraftBySalesOrderNumber(salesOrderData.Number);

            if (invoiceData is null && salesOrderData.AutoInvoice)
            {
                invoiceData = new AutomaticInvoice(salesOrderData, _mapper);
                await _unitOfWork.InvoiceRepository.Add(invoiceData);
            }
            else if (invoiceData is null)
            {
                invoiceData = new PeriodicInvoice(salesOrderData, _mapper);
                await _unitOfWork.InvoiceRepository.Add(invoiceData);
            }
            else if (invoiceData.Type == InvoiceType.Automatic)
            {
                var autoInvoice = new AutomaticInvoice();
                autoInvoice.Initialize(invoiceData, _mapper);

                if (autoInvoice.SalesOrder.Number != salesOrderData.Number)
                {
                    throw new BusinessRuleException($"Automatic invoice {autoInvoice.Number} connected to salesOrder {autoInvoice.SalesOrder.Number}");
                }

                autoInvoice.SalesOrder.Initialize(salesOrderData, Mapper);

                await _unitOfWork.InvoiceRepository.Update(autoInvoice);
            }
            else
            {
                var periodicInvoice = new PeriodicInvoice();
                periodicInvoice.Initialize(invoiceData, _mapper);
                var salesOrder = periodicInvoice.SalesOrders.FirstOrDefault(so => so.Number ==  salesOrderData.Number);
                if(salesOrder is null)
                {
                    periodicInvoice.AddSalesOrder(salesOrderData);
                }
                else
                {
                    salesOrder.Initialize(salesOrderData, Mapper);
                }

                await _unitOfWork.InvoiceRepository.Update(periodicInvoice);
            }

            return invoiceData;
        }

        protected IMapper Mapper => _mapper ?? throw new ArgumentNullException(nameof(_mapper));

        public async Task<IInvoiceData?> GetInvoice(string number)
        {
            return await _unitOfWork.InvoiceRepository.GetByNumber(number);
        }

        public async Task<IInvoiceData> UpdateInvoice(IWarehouseReleaseData warehouseReleaseData)
        {
            var invoiceData = await _unitOfWork.InvoiceRepository.GetDraftBySalesOrderNumber(warehouseReleaseData.SalesOrderNumber);
            var alreadyInvoiced = await _unitOfWork.InvoiceRepository.SalesOrderInvoiced(warehouseReleaseData.SalesOrderNumber);

            if (alreadyInvoiced) { throw new BusinessRuleException($"salesOrder {warehouseReleaseData.SalesOrderNumber} already invoiced"); }
            if (invoiceData is null) { throw new BusinessRuleException($"invalid salesOrderNumber - {warehouseReleaseData.SalesOrderNumber}"); }

            var invoice = invoiceData.Type == InvoiceType.Automatic ? new AutomaticInvoice() 
                : new PeriodicInvoice() as Domain.InvoiceAggregate.Invoice;

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

        public async Task SaveChanges()
        {
            await _unitOfWork.SaveChanges();
        }

        public async Task<IInvoiceData?> EndPeriod(Guid customerId)
        {
            var invoiceData = await _unitOfWork.InvoiceRepository.GetPeriodicDraftByCustomer(customerId);
            if (invoiceData == null) { return null; }

            var invoice = new PeriodicInvoice();
            invoice.Initialize(invoiceData, _mapper);

            invoice.EndPeriod();

            if (invoice.IsReadyToComplete()) {
                invoice.Complete();
                await _eventBus.PublishIntegrationEvent(new PrintInvoiceRequested
                {
                    Invoice = invoice,
                });
            }

            await _unitOfWork.InvoiceRepository.Update(invoice);

            return invoice;
        }
    }
}
