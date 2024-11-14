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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace InvoiceSample.Persistence.Tables
{
    public class InvoiceLine : Entity<InvoiceLine, (string InvoiceNumber, int Ordinal), IInvoiceLine>, IInvoiceLine
    {
        public InvoiceLine()
        {
            RegisterExternalChild(
                WarehouseReleaseLine
                , d => d.WarehouseReleaseLine
                , (_) => { WarehouseReleaseLine = null; }
                , (e) => { WarehouseReleaseLine = (WarehouseMovementLine)e; }
                , (data) => new WarehouseMovementLine { WarehouseMovement = Invoice!.SalesOrders.SelectMany(so => so.WarehouseReleases)
                    .First(wr => wr.Number == data.WarehouseReleaseLine!.WarehouseRelease.Number)
                }
                , (_) => _mapper!
                );

            RegisterExternalChild(
                SalesOrderLine
                , d => d.SalesOrderLine
                , (_) => { SalesOrderLine = null; }
                , (e) => { SalesOrderLine = (SalesOrderLine)e; }
                , (data) => new SalesOrderLine { SalesOrder = Invoice!.SalesOrders.First(so => so.Number == data.SalesOrderLine!.Document.Number) }
                , (_) => _mapper!
                );
        }

        public required Invoice Invoice { get; set; }
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

        public override IInvoiceLine GetEntityData() => this;

        public override object GetKey() => (Invoice.Number, Ordinal);

        (string InvoiceNumber, int Ordinal) IEntityData<(string InvoiceNumber, int Ordinal)>.GetKey()
        {
            return (Invoice.Number, Ordinal);
        }
    }
}
