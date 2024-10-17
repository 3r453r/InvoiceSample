using InvoiceSample.Domain;
using InvoiceSample.Domain.SalesOrderAggregate;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Persistence.Tables
{
    public class SalesOrderLine : Entity, ISalesOrderLine
    {
        public required SalesOrder SalesOrder { get; set; }
        ISalesOrderData ISalesOrderLine.SalesOrder => SalesOrder;
        public IDocument Document => SalesOrder;

        public bool IsService { get; set; }

        public int Ordinal { get; set; }

        [Precision(18,2)]
        public decimal NetValue { get; set; }

        [Precision(18, 2)]
        public decimal VatValue { get; set; }

        [Precision(18, 2)]
        public decimal GrossValue { get; set; }

        public Guid ProductId { get; set; }

        [Precision(18, 6)]
        public decimal Quantity { get; set; }

        public VatRate VatRate { get; set; }

        public bool IsEqual(ISalesOrderLine? other) => other is not null &&
            SalesOrder.Number == other.SalesOrder.Number && Ordinal == other.Ordinal;
    }
}
