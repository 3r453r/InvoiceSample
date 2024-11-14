using InvoiceSample.DataDrivenEntity;
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
    public class SalesOrderLine : Entity<SalesOrderLine, (string SalesOrderNumber, int Ordinal), ISalesOrderLine>, ISalesOrderLine
    {
        public required SalesOrder SalesOrder { get; set; }
        ISalesOrderData ISalesOrderLine.SalesOrder => SalesOrder;
        public IDocument Document => SalesOrder;

        public bool IsService { get; set; }

        public int Ordinal { get; set; }

        [Precision(18, 2)]
        public decimal NetValue { get; set; }

        [Precision(18, 2)]
        public decimal VatValue { get; set; }

        [Precision(18, 2)]
        public decimal GrossValue { get; set; }

        public Guid ProductId { get; set; }

        [Precision(18, 6)]
        public decimal Quantity { get; set; }

        public VatRate VatRate { get; set; }

        object IEntityData.GetKey() => (SalesOrder.Number, Ordinal);

        public override ISalesOrderLine GetEntityData() => this;

        public override object GetKey() => (SalesOrder.Number, Ordinal);

        (string SalesOrderNumber, int Ordinal) IEntityData<(string SalesOrderNumber, int Ordinal)>.GetKey()
        {
            return (SalesOrder.Number, Ordinal);
        }
    }
}
