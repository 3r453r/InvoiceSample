using InvoiceSample.DataDrivenEntity;
using InvoiceSample.Domain;
using InvoiceSample.Domain.WarehouseReleaseAggregate;
using System.ComponentModel.DataAnnotations;

namespace InvoiceSample.WebApi.Dtos
{
    public class WarehouseReleaseRequested : IWarehouseReleaseData
    {
        [Required]
        public required string SalesOrderNumber { get; set; }

        public List<WarehouseReleaseLine> Lines { get; set; } = [];
        IEnumerable<IWarehouseReleaseLine> IWarehouseReleaseData.Lines => Lines.Select(l => new WarehouseReleaseLineData(this, l));
        IEnumerable<IDocumentLine> IDocument.Lines => Lines.Select(l => new WarehouseReleaseLineData(this, l));

        [Required]
        public required string Number { get; set; }

        [Required]
        public Guid CustomerId { get; set; }

        public class WarehouseReleaseLineData : IWarehouseReleaseLine
        {
            public WarehouseReleaseLineData(WarehouseReleaseRequested warehouseRelease, WarehouseReleaseLine warehouseReleaseLine)
            {
                WarehouseRelease = warehouseRelease;
                SalesOrderLineOrdinal = warehouseReleaseLine.SalesOrderLineOrdinal;
                Ordinal = warehouseReleaseLine.Ordinal;
                NetValue = warehouseReleaseLine.NetValue;
                VatValue = warehouseReleaseLine.VatValue;
                GrossValue = warehouseReleaseLine.GrossValue;
                ProductId = warehouseReleaseLine.ProductId;
                Quantity = warehouseReleaseLine.Quantity;
                VatRate = warehouseReleaseLine.VatRate;
            }

            public IWarehouseReleaseData WarehouseRelease { get; set; }

            public int? SalesOrderLineOrdinal { get; set; }

            public int Ordinal { get; set; }

            public decimal NetValue { get; set; }

            public decimal VatValue { get; set; }

            public decimal GrossValue { get; set; }

            public Guid ProductId { get; set; }

            public decimal Quantity { get; set; }

            public VatRate VatRate { get; set; }

            public IDocument Document => WarehouseRelease;

            public (string WarehouseReleaseNumber, int Ordinal) GetKey() => (WarehouseRelease.Number, Ordinal);

            object IEntityData.GetKey() => (WarehouseRelease.Number, Ordinal);
        }

        public class WarehouseReleaseLine
        {
            public int? SalesOrderLineOrdinal { get; set; }

            [Required]
            public int Ordinal { get; set; }

            [Required]
            public decimal NetValue { get; set; }

            [Required]
            public decimal VatValue { get; set; }

            [Required]
            public decimal GrossValue { get; set; }

            [Required]
            public Guid ProductId { get; set; }

            [Required]
            public decimal Quantity { get; set; }

            [Required]
            public VatRate VatRate { get; set; }
        }

        string IEntityData<string>.GetKey() => Number;

        object IEntityData.GetKey() => Number;
    }
}
