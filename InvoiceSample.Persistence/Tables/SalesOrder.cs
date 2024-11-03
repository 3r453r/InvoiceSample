using AutoMapper;
using InvoiceSample.DataDrivenEntity;
using InvoiceSample.Domain;
using InvoiceSample.Domain.InvoiceAggregate;
using InvoiceSample.Domain.SalesOrderAggregate;
using InvoiceSample.Domain.WarehouseReleaseAggregate;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Persistence.Tables
{
    public class SalesOrder : Entity<SalesOrder, string, ISalesOrderData>, ISalesOrderData
    {
        public SalesOrder()
        {
            RegisterExternalChildCollection<SalesOrderLine, int, ISalesOrderLine, IMapper>(
                    Lines
                    , d => d.Lines
                    , (_, _) => new SalesOrderLine()
                    , (_) => _mapper!
                );

            RegisterExternalChildCollection<WarehouseMovement, string, IWarehouseReleaseData, IMapper>(
                    WarehouseReleases
                    , d => d.WarehouseReleases
                    , (_, _) => new WarehouseMovement()
                    , (_) => _mapper!
                );

            RegisterExternalChildCollection<Invoice, Guid, IInvoiceData, IMapper>(
                    Invoices
                    , d => d.Invoices
                    , (_, _) => new Invoice()
                    , (_) => _mapper!
                );
        }

        public bool AutoInvoice { get; set; }

        public bool ServiceLinesInvoiced { get; set; }

        [MaxLength(50)]
        public string Number { get; set; } = "";

        public Guid CustomerId { get; set; }

        public List<SalesOrderLine> Lines { get; set; } = [];
        IEnumerable<ISalesOrderLine> ISalesOrderData.Lines => Lines;
        IEnumerable<IDocumentLine> IDocument.Lines => Lines;

        public List<WarehouseMovement> WarehouseReleases { get; set; } = [];
        IEnumerable<IWarehouseReleaseData> ISalesOrderData.WarehouseReleases => WarehouseReleases;

        public List<Invoice> Invoices { get; set; } = [];
        IEnumerable<Domain.InvoiceAggregate.IInvoiceData> ISalesOrderData.Invoices => Invoices;

        string IEntityData<string>.GetKey() => Number;

        object IEntityData.GetKey() => Number;

        public override ISalesOrderData GetEntityData() => this;

        public override object GetKey() => Number;
    }
}
