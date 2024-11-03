using AutoMapper;
using InvoiceSample.DataDrivenEntity;
using InvoiceSample.Domain;
using InvoiceSample.Domain.InvoiceAggregate;
using InvoiceSample.Domain.SalesOrderAggregate;
using InvoiceSample.Domain.WarehouseReleaseAggregate;
using InvoiceSample.Persistence.Extensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Persistence.Tables
{
    public class Invoice : Entity<Invoice, Guid, IInvoiceData>, IInvoiceData
    {
        public Invoice()
        {
            RegisterExternalChildCollection<InvoiceLine, int, IInvoiceLine, IMapper>(
        Lines
        , d => d.Lines
        , (_, _) => new InvoiceLine()
        , (_) => _mapper!
    );

            RegisterExternalChildCollection<InvoiceVatSum, VatRate, IVatSum, IMapper>(
                    VatSums
                    , d => d.VatSums
                    , (_, _) => new InvoiceVatSum()
                    , (_) => _mapper!
                );

            RegisterExternalChildCollection<SalesOrder, string, ISalesOrderData, IMapper>(
                    SalesOrders
                    , d => d.SalesOrders
                    , (_, _) => new SalesOrder()
                    , (_) => _mapper!
                );
        }

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

        public string Number { get; set; } = "";

        public Guid CustomerId { get; set; }

        public override IInvoiceData GetEntityData() => this;

        Guid IEntityData<Guid>.GetKey() => Id;

        public override object GetKey() => Id;
    }
}
