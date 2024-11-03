using AutoMapper;
using InvoiceSample.Domain.SalesOrderAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Domain.InvoiceAggregate
{
    public class PeriodicInvoice : Invoice
    {
        public PeriodicInvoice() : base()
        {
        }

        public PeriodicInvoice(ISalesOrderData salesOrderData, IMapper mapper) : base(salesOrderData, mapper)
        {
        }

        public bool PeriodEndRequested { get; private set; }

        public override void Complete()
        {
            State = InvoiceState.ReadyToInvoice;

            var i = 1;
            var maxOrdinal = _lines.Any() ? _lines.Max(l => l.Ordinal) : 0;

            foreach (var salesOrderLine in _salesOrders
                .Where(so => !so.ServiceLinesInvoiced)
                .SelectMany(so => so.Lines.Where(l => l.IsService)))
            {
                salesOrderLine.SalesOrder.ServiceLinesInvoiced = true;
                _lines.Add(new InvoiceLine
                {
                    Invoice = this,
                    SalesOrderLine = salesOrderLine,
                    GrossValue = salesOrderLine.GrossValue,
                    NetValue = salesOrderLine.NetValue,
                    Ordinal = maxOrdinal + i++,
                    ProductId = salesOrderLine.ProductId,
                    Quantity = salesOrderLine.Quantity,
                    VatRate = salesOrderLine.VatRate,
                    VatValue = salesOrderLine.VatValue,
                });
            }

            UpdateTotals();
        }

        public override bool IsReadyToComplete()
        {
            return (_lines.Any() || _salesOrders.Any(so => !so.ServiceLinesInvoiced)) && PeriodEndRequested;
        }

        public void AddSalesOrder(ISalesOrderData salesOrderData)
        {
            var salesOrder = new SalesOrder();
            salesOrder.Initialize(salesOrderData, Mapper);
            _salesOrders.Add(salesOrder);
        }

        public void EndPeriod()
        {
            PeriodEndRequested = true;
        }
    }
}
