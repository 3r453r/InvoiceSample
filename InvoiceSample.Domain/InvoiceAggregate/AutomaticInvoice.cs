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
        private readonly SalesOrder _salesOrder;

        public AutomaticInvoice(IInvoiceData invoiceData) : base(invoiceData)
        {
            if (_salesOrders.Count != 1)
                throw new ArgumentException("automatic invoice can only have one salesOrder");

            _salesOrder = _salesOrders.First();
        }

        public AutomaticInvoice(ISalesOrderData salesOrderData) : base(salesOrderData)
        {
            _salesOrder = _salesOrders.First();
        }

        public SalesOrder SalesOrder => _salesOrder;

        public override void Complete()
        {
            State = InvoiceState.ReadyToInvoice;

            _salesOrder.ServiceLinesInvoiced = true;

            var i = 1;
            var maxOrdinal = _lines.Any() ? _lines.Max(l => l.Ordinal) : 0;

            foreach (var salesOrderLine in _salesOrder.Lines.Where(l => l.IsService))
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
            return _salesOrder.IsCompleted();
        }
    }
}
