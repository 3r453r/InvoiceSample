using InvoiceSample.Domain;
using InvoiceSample.Domain.SalesOrderAggregate;
using InvoiceSample.Domain.WarehouseReleaseAggregate;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Persistence.Tables
{
    public class SalesOrder : Entity, ISalesOrderData
    {
        public bool AutoInvoice { get; set; }

        public bool ServiceLinesInvoiced { get; set; }

        [MaxLength(50)]
        public required string Number { get; set; }

        public Guid CustomerId { get; set; }

        public List<SalesOrderLine> Lines { get; set; } = [];
        IEnumerable<ISalesOrderLine> ISalesOrderData.Lines => Lines;
        IEnumerable<IDocumentLine> IDocument.Lines => Lines;

        public List<WarehouseMovement> WarehouseReleases { get; set; } = [];
        IEnumerable<IWarehouseReleaseData> ISalesOrderData.WarehouseReleases => WarehouseReleases;

        public List<Invoice> Invoices { get; set; } = [];
    }
}
