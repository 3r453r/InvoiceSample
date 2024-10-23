using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Domain.InvoiceAggregate
{
    public interface IVatSum
    {
        VatRate VatRate { get; }
        decimal NetValue { get; }
        decimal GrossValue { get; }
        decimal VatValue { get; }
    }

    public class VatSum : IVatSum
    {
        public static VatSum Create(IVatSum vatSumData) => new VatSum
        {
            VatRate = vatSumData.VatRate,
            GrossValue = vatSumData.GrossValue,
            NetValue = vatSumData.NetValue,
            VatValue = vatSumData.VatValue
        };

        public VatRate VatRate { get; set; }

        public decimal NetValue { get; set; }
        public decimal GrossValue { get; set; }
        public decimal VatValue { get; set; }

        public void UpdateCollections(IVatSum entityData)
        {
        }

        public void UpdateEntity(IVatSum entityData)
        {
            NetValue = entityData.NetValue;
            GrossValue = entityData.GrossValue;
            VatValue = entityData.VatValue;
        }
    }
}
