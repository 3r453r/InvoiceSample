using InvoiceSample.Domain.Exceptions;
using InvoiceSample.Domain.WarehouseReleaseAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Domain.SalesOrderAggregate
{
    public class SalesOrder : ISalesOrderData
    {
        private readonly List<WarehouseRelease> _warehouseReleases = [];
        public SalesOrder(ISalesOrderData salesOrderData)
        {
            AutoInvoice = salesOrderData.AutoInvoice;
            Number = salesOrderData.Number;
            CustomerId = salesOrderData.CustomerId;

            Lines = salesOrderData.Lines.Select(l => new SalesOrderLine
            {
                SalesOrder = this,
                GrossValue = l.GrossValue,
                NetValue = l.NetValue,
                Ordinal = l.Ordinal,
                ProductId = l.ProductId,
                Quantity = l.Quantity,
                VatRate = l.VatRate,
                VatValue = l.VatValue,
                IsService = l.IsService,
            }).ToArray();

            foreach (var wr in salesOrderData.WarehouseReleases)
            {
                WarehouseReleaseIssued(wr);
            }
        }

        public WarehouseRelease WarehouseReleaseIssued(IWarehouseReleaseData warehouseReleaseData)
        {
            if (warehouseReleaseData.SalesOrderNumber != Number)
                throw new BusinessRuleException("invalid salesOrder for WR");

            if (_warehouseReleases.Any(wr => wr.Number == warehouseReleaseData.Number))
                throw new BusinessRuleException("WR already in salesOrder");

            var warehouseRelease = new WarehouseRelease(warehouseReleaseData);
            _warehouseReleases.Add(warehouseRelease);

            return warehouseRelease;
        }

        public bool IsCompleted()
        {
            var productDeliveries = new Dictionary<Guid, decimal>();
            foreach(var wrLine in _warehouseReleases.SelectMany(wr => wr.Lines))
            {
                if(!productDeliveries.TryAdd(wrLine.ProductId, wrLine.Quantity))
                {
                    productDeliveries[wrLine.ProductId] += wrLine.Quantity;
                }
            }

            foreach(var line in Lines.Where(l => !l.IsService))
            {
                var deliveredQuantity = 0m;
                productDeliveries.TryGetValue(line.ProductId, out deliveredQuantity);
                if( deliveredQuantity < line.Quantity)
                {
                    return false;
                }
            }

            return true;
        }

        public void Update(ISalesOrderData salesOrderData)
        {
            if(AutoInvoice != salesOrderData.AutoInvoice)
            {
                throw new BusinessRuleException("Changing SO type not allowed");
            }
            //Other updates+
        }

        public bool AutoInvoice { get; set; }

        public string Number { get; set; }

        public Guid CustomerId { get; set; }

        public bool ServiceLinesInvoiced { get; set; }

        public IEnumerable<SalesOrderLine> Lines { get; set; }
        IEnumerable<IDocumentLine> IDocument.Lines => Lines;
        IEnumerable<ISalesOrderLine> ISalesOrderData.Lines => Lines;

        public IEnumerable<WarehouseRelease> WarehouseReleases => _warehouseReleases;
        IEnumerable<IWarehouseReleaseData> ISalesOrderData.WarehouseReleases => _warehouseReleases;
    }
}
