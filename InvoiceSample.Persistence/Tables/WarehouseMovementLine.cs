using InvoiceSample.Domain;
using InvoiceSample.Domain.WarehouseReleaseAggregate;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Persistence.Tables
{
    public class WarehouseMovementLine : Entity, IWarehouseReleaseLine
    {
        public required WarehouseMovement WarehouseMovement { get; set; }
        IWarehouseReleaseData IWarehouseReleaseLine.WarehouseRelease => WarehouseMovement;
        public IDocument Document => WarehouseMovement;

        public SalesOrderLine? SalesOrderLine { get; set; }
        public int? SalesOrderLineOrdinal => SalesOrderLine?.Ordinal ?? 0;

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

        public bool IsEqual(IWarehouseReleaseLine? other) => other is not null &&
            WarehouseMovement.Number == other.WarehouseRelease.Number && Ordinal == other.Ordinal;
    }
}
