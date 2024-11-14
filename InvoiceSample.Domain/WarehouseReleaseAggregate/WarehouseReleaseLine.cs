using AutoMapper;
using InvoiceSample.DataDrivenEntity;
using InvoiceSample.DataDrivenEntity.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Domain.WarehouseReleaseAggregate
{
    public class WarehouseReleaseLine : ExternalDataDrivenEntity<(string WarehouseReleaseNumber, int Ordinal), IWarehouseReleaseLine, WarehouseReleaseLineExternalData>
        , IWarehouseReleaseLine 
    {
        public WarehouseReleaseLine(WarehouseRelease warehouseRelease)
        {
            WarehouseRelease = warehouseRelease;
        }

        private bool _initialized;

        public int Ordinal { get; set; }

        public decimal NetValue { get; set; }

        public decimal VatValue { get; set; }

        public decimal GrossValue { get; set; }

        public Guid ProductId { get; set; }

        public decimal Quantity { get; set; }

        public VatRate VatRate { get; set; }

        public int? SalesOrderLineOrdinal { get; set; }

        public WarehouseRelease WarehouseRelease { get; private set; }
        public IDocument Document => WarehouseRelease;

        protected override bool SelfInitialzed => _initialized;

        IWarehouseReleaseData IWarehouseReleaseLine.WarehouseRelease => WarehouseRelease;

        public override IWarehouseReleaseLine GetEntityData() => this;

        public override (string WarehouseReleaseNumber, int Ordinal) GetKey() => (WarehouseRelease.Number, Ordinal);
        object IEntityData.GetKey() => Ordinal;

        protected override void SelfInitialize(IWarehouseReleaseLine entityData
            , WarehouseReleaseLineExternalData externalData)
        {
            externalData.Mapper.Map(entityData, this);
            _initialized = true;
        }
    }

    public record WarehouseReleaseLineExternalData
    {
        public required IMapper Mapper { get; init; }
    }
}
