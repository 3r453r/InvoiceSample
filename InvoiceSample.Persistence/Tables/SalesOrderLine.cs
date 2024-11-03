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
    public class SalesOrderLine : Entity<SalesOrderLine, int, ISalesOrderLine>, ISalesOrderLine
    {
        public SalesOrder SalesOrder { get; set; } = new SalesOrder();
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

        object IEntityData.GetKey() => Ordinal;

        public override ISalesOrderLine GetEntityData() => this;

        public override object GetKey() => Ordinal;

        int IEntityData<int>.GetKey() => Ordinal;
    }
}
