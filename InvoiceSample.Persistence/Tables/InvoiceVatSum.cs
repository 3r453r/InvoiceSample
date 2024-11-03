using InvoiceSample.DataDrivenEntity;
using InvoiceSample.Domain;
using InvoiceSample.Domain.InvoiceAggregate;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Persistence.Tables
{
    public class InvoiceVatSum : Entity<InvoiceVatSum, VatRate, IVatSum>, IVatSum
    {
        public Invoice Invoice { get; set; } = new Invoice();

        public VatRate VatRate { get; set; }

        [Precision(18,2)]
        public decimal NetValue { get; set; }

        [Precision(18, 2)]
        public decimal GrossValue { get; set; }

        [Precision(18, 2)]
        public decimal VatValue { get; set; }

        public override IVatSum GetEntityData() => this;


        public override object GetKey() => VatRate;

        object IEntityData.GetKey() => VatRate;

        VatRate IEntityData<VatRate>.GetKey() => VatRate;
    }
}
