using InvoiceSample.Domain;
using InvoiceSample.Domain.InvoiceAggregate;
using InvoiceSample.Domain.SalesOrderAggregate;
using InvoiceSample.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Persistence.Tables
{
    public class Invoice : Entity, IInvoiceData
    {
        [Precision(18,2)]
        public decimal NetValue { get; set; }

        [Precision(18, 2)]

        public decimal VatValue { get; set; }

        [Precision(18, 2)]
        public decimal GrossValue { get; set; }

        public InvoiceState State { get; set; }

        public InvoiceType Type { get; set; }

        public List<InvoiceLine> Lines { get; set; } = [];
        IEnumerable<IInvoiceLine> IInvoiceData.Lines => Lines;
        IEnumerable<IDocumentLine> IDocument.Lines => Lines;

        public List<InvoiceVatSum> VatSums { get; set; } = [];
        IEnumerable<IVatSum> IInvoiceData.VatSums => VatSums;

        public List<SalesOrder> SalesOrders { get; set; } = [];
        IEnumerable<ISalesOrderData> IInvoiceData.SalesOrders => SalesOrders;

        public required string Number { get; set; }

        public Guid CustomerId { get; set; }

        public override void UpdateCollections<TEntityData>(TEntityData entityData, DbContext dbContext)
        {
            if (entityData is IInvoiceData invoiceData)
            {
                SalesOrders.UpdateCollection(
                    invoiceData.SalesOrders
                    , dbContext
                    , SalesOrder.CreateFromData
                    , (e, ed) => e.UpdateFromData(ed)
                    , so => so.Number
                    , sod => sod.Number
                    );

                Lines.UpdateCollection(
                    invoiceData.Lines
                    , dbContext
                    , l => InvoiceLine.CreateFromData(l, this)
                    , (e, ed) => e.UpdateFromData(ed, this)
                    , l => l.Ordinal
                    , ld => ld.Ordinal);
            }
            else
            {
                throw new ArgumentException("invalid entity data type");
            }
        }
    }
}
