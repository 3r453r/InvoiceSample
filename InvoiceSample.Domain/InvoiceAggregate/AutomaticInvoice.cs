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
            return SalesOrder.IsCompleted();
        }
    }
}
