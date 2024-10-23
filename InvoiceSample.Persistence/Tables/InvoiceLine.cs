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
    public class InvoiceLine : Entity, IInvoiceLine
    {
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

        public override void UpdateCollections<TEntityData>(TEntityData entityData, DbContext dbContext)
        {
        }

        public static InvoiceLine CreateFromData(IInvoiceLine invoiceLine, Invoice invoice)
        {
            var salesOrderLine = invoice.SalesOrders
                .FirstOrDefault(so => invoiceLine.SalesOrderLine?.SalesOrder.Number == so.Number)
                ?.Lines.FirstOrDefault(l => l.Ordinal == invoiceLine.SalesOrderLine?.Ordinal);

            var warehouseReleaseLine = invoice.SalesOrders.SelectMany(so => so.WarehouseReleases)
                .FirstOrDefault(wr => wr.Number == invoiceLine.WarehouseReleaseLine?.WarehouseRelease.Number)
                ?.Lines.FirstOrDefault(l => l.Ordinal == invoiceLine.WarehouseReleaseLine?.Ordinal);

            return new InvoiceLine
            {
                Invoice = invoice,
                GrossValue = invoice.GrossValue,
                NetValue = invoiceLine.NetValue,
                Ordinal = invoiceLine.Ordinal,
                ProductId = invoiceLine.ProductId,
                Quantity = invoiceLine.Quantity,
                VatRate = invoiceLine.VatRate,
                VatValue = invoiceLine.VatValue,
                SalesOrderLine = salesOrderLine,
                WarehouseReleaseLine = warehouseReleaseLine,
            };
        }

        public void UpdateFromData(IInvoiceLine invoiceLineData, Invoice invoice)
        {
            //TODO implement
        }
    }
}
