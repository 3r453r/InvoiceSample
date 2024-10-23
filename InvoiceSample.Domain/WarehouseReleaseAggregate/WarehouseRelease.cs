using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Domain.WarehouseReleaseAggregate
{
    public class WarehouseRelease : IWarehouseReleaseData
    {
        public static WarehouseRelease Create(IWarehouseReleaseData warehouseReleaseData) => new WarehouseRelease(warehouseReleaseData);

        public WarehouseRelease(IWarehouseReleaseData warehouseReleaseData)
        {
            Number = warehouseReleaseData.Number;
            CustomerId = warehouseReleaseData.CustomerId;
            SalesOrderNumber = warehouseReleaseData.SalesOrderNumber;

            Lines.AddRange(warehouseReleaseData.Lines.Select(l => new WarehouseReleaseLine
            {
                WarehouseRelease = this,
                GrossValue = l.GrossValue,
                NetValue = l.NetValue,
                Ordinal = l.Ordinal,
                ProductId = l.ProductId,
                Quantity = l.Quantity,
                SalesOrderLineOrdinal = l.SalesOrderLineOrdinal,
                VatRate = l.VatRate,
                VatValue = l.VatValue,
            }));
        }

        public string Number { get; private set; }

        public Guid CustomerId { get; private set; }

        public string SalesOrderNumber { get; private set; }

        public List<WarehouseReleaseLine> Lines { get; init; } = [];
        IEnumerable<IDocumentLine> IDocument.Lines => Lines;
        IEnumerable<IWarehouseReleaseLine> IWarehouseReleaseData.Lines => Lines;

        public void UpdateCollections(IWarehouseReleaseData entityData)
        {
        }

        public void UpdateEntity(IWarehouseReleaseData entityData)
        {
            Number = entityData.Number;
            CustomerId = entityData.CustomerId;
            SalesOrderNumber = entityData.SalesOrderNumber;
        }

        internal void Update(IWarehouseReleaseData warehouseReleaseData)
        {
            //TODO update
        }
    }
}
