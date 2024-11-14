using AutoMapper;
using InvoiceSample.DataDrivenEntity;
using InvoiceSample.Domain;
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
    public class WarehouseMovementLine : Entity<WarehouseMovementLine, (string WarehouseReleaseNumber, int Ordinal), IWarehouseReleaseLine>, IWarehouseReleaseLine
    {
        public WarehouseMovementLine()
        {
        }

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

        public override IWarehouseReleaseLine GetEntityData() => this;

        public override object GetKey() => (WarehouseMovement.Number, Ordinal);

        (string WarehouseReleaseNumber, int Ordinal) IEntityData<(string WarehouseReleaseNumber, int Ordinal)>.GetKey()
        {
            return (WarehouseMovement.Number, Ordinal);
        }
    }
}
