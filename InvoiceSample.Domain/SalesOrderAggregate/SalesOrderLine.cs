using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Domain.SalesOrderAggregate
{
    public record SalesOrderLine : ISalesOrderLine
    {
        public int Ordinal { get; set; }

        public decimal NetValue { get; set; }

        public decimal VatValue { get; set; }

        public decimal GrossValue { get; set; }

        public Guid ProductId { get; set; }

        public decimal Quantity { get; set; }

        public VatRate VatRate { get; set; }

        public bool IsService { get; set; }

        public required SalesOrder SalesOrder { get; set; }
        public IDocument Document => SalesOrder;
        ISalesOrderData ISalesOrderLine.SalesOrder => SalesOrder;
    }
}
