using InvoiceSample.Domain.SalesOrderAggregate;
using InvoiceSample.Domain.WarehouseReleaseAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Domain.InvoiceAggregate
{
    public class InvoiceLine : IInvoiceLine
    {
        public int Ordinal { get; set; }

        public decimal NetValue { get; set; }

        public decimal VatValue { get; set; }

        public decimal GrossValue { get; set; }

        public Guid ProductId { get; set; }

        public decimal Quantity { get; set; }

        public VatRate VatRate { get; set; }

        public WarehouseReleaseLine? WarehouseReleaseLine { get; set; }
        IWarehouseReleaseLine? IInvoiceLine.WarehouseReleaseLine => WarehouseReleaseLine;

        public required Invoice Invoice { get; set; }
        public IDocument Document => Invoice;
        IInvoiceData IInvoiceLine.Invoice => Invoice;

        public SalesOrderLine? SalesOrderLine { get; set; }
        ISalesOrderLine? IInvoiceLine.SalesOrderLine => SalesOrderLine;
    }
}
