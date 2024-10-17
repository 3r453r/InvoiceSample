using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Domain.WarehouseReleaseAggregate
{
    public record WarehouseReleaseLine : IWarehouseReleaseLine
    {
        public int Ordinal { get; set; }

        public decimal NetValue { get; set; }

        public decimal VatValue { get; set; }

        public decimal GrossValue { get; set; }

        public Guid ProductId { get; set; }

        public decimal Quantity { get; set; }

        public VatRate VatRate { get; set; }

        public int? SalesOrderLineOrdinal { get; set; }

        public required WarehouseRelease WarehouseRelease { get; init; }
        public IDocument Document => WarehouseRelease;
        IWarehouseReleaseData IWarehouseReleaseLine.WarehouseRelease => WarehouseRelease;
    }
}
