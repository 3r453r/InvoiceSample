using InvoiceSample.Domain;
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
    public class WarehouseMovement : Entity, IWarehouseReleaseData
    {
        public SalesOrder? SalesOrder { get; set; }
        public string SalesOrderNumber => SalesOrder?.Number ?? "";

        public List<WarehouseMovementLine> Lines { get; set; } = [];
        IEnumerable<IWarehouseReleaseLine> IWarehouseReleaseData.Lines => Lines;
        IEnumerable<IDocumentLine> IDocument.Lines => Lines;

        [MaxLength(50)]
        public required string Number { get; set; }

        public Guid CustomerId { get; set; }

        public WarehouseMovementType Type { get; set; }

        public override void UpdateCollections<TEntityData>(TEntityData entityData, DbContext dbContext)
        {
        }
    }

    public enum WarehouseMovementType : byte
    {
        WarehouseRelease,
        WarehouseReturn
    }
}
