using AutoMapper;
using InvoiceSample.Domain.SalesOrderAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Domain.InvoiceAggregate
{
    public class AutomaticInvoice : Invoice
    {
        public AutomaticInvoice() : base()
        {
        }

        public AutomaticInvoice(ISalesOrderData salesOrderData, IMapper mapper) : base(salesOrderData, mapper)
        {
        }

        public SalesOrder SalesOrder => _salesOrders.First();

        public override void Complete()
        {
            State = InvoiceState.ReadyToInvoice;

            SalesOrder.ServiceLinesInvoiced = true;

            var i = 1;
            var maxOrdinal = _lines.Any() ? _lines.Max(l => l.Ordinal) : 0;

            foreach (var salesOrderLine in SalesOrder.Lines.Where(l => l.IsService))
            {
                var invoiceLine = new InvoiceLine(this);
                invoiceLine.SalesOrderLine = salesOrderLine;
                invoiceLine.GrossValue = salesOrderLine.GrossValue;
                invoiceLine.NetValue = salesOrderLine.NetValue;
                invoiceLine.Ordinal = maxOrdinal + i++;
                invoiceLine.ProductId = salesOrderLine.ProductId;
                invoiceLine.Quantity = salesOrderLine.Quantity;
                invoiceLine.VatRate = salesOrderLine.VatRate;
                invoiceLine.VatValue = salesOrderLine.VatValue; 
                _lines.Add(invoiceLine);              
            }

            UpdateTotals();
        }

        public override bool IsReadyToComplete()
        {
            return SalesOrder.IsCompleted();
        }
    }
}
