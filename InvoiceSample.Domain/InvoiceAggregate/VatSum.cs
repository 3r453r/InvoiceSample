using AutoMapper;
using InvoiceSample.DataDrivenEntity;
using InvoiceSample.DataDrivenEntity.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Domain.InvoiceAggregate
{
    public interface IVatSum : IEntityData<(string InvoiceNumber, VatRate VatRate)>
    {
        VatRate VatRate { get; }
        decimal NetValue { get; }
        decimal GrossValue { get; }
        decimal VatValue { get; }
    }

    public class VatSum : ExternalDataDrivenEntity<(string InvoiceNumber, VatRate VatRate), IVatSum, IMapper>
        , IVatSum
    {
        private bool _initialized;

        public VatSum(Invoice invoice)
        {
            Invoice = invoice;
        }

        public Invoice Invoice { get; set; }
        public VatRate VatRate { get; set; }

        public decimal NetValue { get; set; }
        public decimal GrossValue { get; set; }
        public decimal VatValue { get; set; }

        protected override bool SelfInitialzed => _initialized;

        public override IVatSum GetEntityData() => this;

        public override (string InvoiceNumber, VatRate VatRate) GetKey() => (Invoice.Number, VatRate  = VatRate);

        protected override void SelfInitialize(IVatSum entityData, IMapper externalData)
        {
            externalData.Map(entityData, this);
            _initialized = true;
        }

        object IEntityData.GetKey() => VatRate;
    }
}
