using AutoMapper;
using InvoiceSample.DataDrivenEntity;
using InvoiceSample.Domain;
using InvoiceSample.Domain.InvoiceAggregate;
using InvoiceSample.Domain.SalesOrderAggregate;
using InvoiceSample.Domain.WarehouseReleaseAggregate;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Persistence.Tables
{
    public class InvoiceLine : Entity<InvoiceLine, int, IInvoiceLine>, IInvoiceLine
    {
        public InvoiceLine()
        {
            RegisterExternalChild<WarehouseMovementLine, int, IWarehouseReleaseLine, IMapper>(
                WarehouseReleaseLine
                , d => d.WarehouseReleaseLine
                , (_) => { WarehouseReleaseLine = null; }
                , (e) => { WarehouseReleaseLine = (WarehouseMovementLine)e; }
                , (_) => new WarehouseMovementLine()
                , (_) => _mapper!
                );

            RegisterExternalChild<SalesOrderLine, int, ISalesOrderLine, IMapper>(
                SalesOrderLine
                , d => d.SalesOrderLine
                , (_) => { SalesOrderLine = null; }
                , (e) => { SalesOrderLine = (SalesOrderLine)e; }
                , (_) => new SalesOrderLine()
                , (_) => _mapper!
                );
        }

        public Invoice Invoice { get; set; } = new Invoice();
        public IDocument Document => Invoice;
        IInvoiceData IInvoiceLine.Invoice => Invoice;

        public WarehouseMovementLine? WarehouseReleaseLine { get; set; }
        IWarehouseReleaseLine? IInvoiceLine.WarehouseReleaseLine => WarehouseReleaseLine;

        public SalesOrderLine? SalesOrderLine { get; set; }
        ISalesOrderLine? IInvoiceLine.SalesOrderLine => SalesOrderLine;

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

        object IEntityData.GetKey() => Ordinal;

        public override IInvoiceLine GetEntityData() => this;

        public override object GetKey() => Ordinal;

        int IEntityData<int>.GetKey() => Ordinal;
    }
}
