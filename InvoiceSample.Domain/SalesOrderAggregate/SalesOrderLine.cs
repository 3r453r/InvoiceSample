using AutoMapper;
using InvoiceSample.DataDrivenEntity;
using InvoiceSample.DataDrivenEntity.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Domain.SalesOrderAggregate
{
    public class SalesOrderLine : ExternalDataDrivenEntity<(string SalesOrderNumber, int Ordinal), ISalesOrderLine, SalesOrderLineExternalData>
        , ISalesOrderLine
    {
        private bool _initialized;

        public SalesOrderLine(SalesOrder salesOrder)
        {
            SalesOrder = salesOrder;
        }

        public int Ordinal { get; set; }

        public decimal NetValue { get; set; }

        public decimal VatValue { get; set; }

        public decimal GrossValue { get; set; }

        public Guid ProductId { get; set; }

        public decimal Quantity { get; set; }

        public VatRate VatRate { get; set; }

        public bool IsService { get; set; }

        public SalesOrder SalesOrder { get; set; }

        public IDocument Document => SalesOrder;

        protected override bool SelfInitialzed => _initialized;

        ISalesOrderData ISalesOrderLine.SalesOrder => SalesOrder;

        public override ISalesOrderLine GetEntityData() => this;

        public override (string SalesOrderNumber, int Ordinal) GetKey() => (SalesOrder.Number, Ordinal);

        protected override void SelfInitialize(ISalesOrderLine entityData, SalesOrderLineExternalData externalData)
        {
            externalData.Mapper.Map(entityData, this);
            _initialized = true;
        }

        object IEntityData.GetKey() => Ordinal;
    }

    public record SalesOrderLineExternalData
    {
        public required IMapper Mapper { get; set; }
    }
}
