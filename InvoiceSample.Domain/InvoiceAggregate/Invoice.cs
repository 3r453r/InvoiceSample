using InvoiceSample.Domain.Exceptions;
using InvoiceSample.Domain.SalesOrderAggregate;
using InvoiceSample.Domain.WarehouseReleaseAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Domain.InvoiceAggregate
{
    public abstract class Invoice : IInvoiceData
    {
        protected readonly List<InvoiceLine> _lines = [];
        protected readonly List<VatSum> _vatSums = [];
        protected readonly List<SalesOrder> _salesOrders = [];

        public Invoice(IInvoiceData invoiceData)
        {
            Number = invoiceData.Number;
            State = invoiceData.State;
            NetValue = invoiceData.NetValue;
            VatValue = invoiceData.VatValue;
            GrossValue = invoiceData.GrossValue;

            foreach (var lineGroup in invoiceData.Lines.GroupBy(l => l.WarehouseReleaseLine?.Document.Number)) 
            { 
                var warehouseReleaseData = lineGroup.First().WarehouseReleaseLine?.WarehouseRelease;
                var warehouseRelease = warehouseReleaseData is not null ? 
                    new WarehouseRelease(warehouseReleaseData) : null;

                var salesOrderData = lineGroup.First(l => l.SalesOrderLine is not null).SalesOrderLine!.SalesOrder;
                var salesOrder = new SalesOrder(salesOrderData);
                if(warehouseRelease?.SalesOrderNumber != salesOrder.Number)
                {
                    throw new ArgumentException("WR doesn't match SO");
                }

                _lines.AddRange(lineGroup.Select(dl => new InvoiceLine
                {
                    GrossValue = dl.GrossValue,
                    NetValue = dl.NetValue,
                    Ordinal = dl.Ordinal,
                    ProductId = dl.ProductId,
                    Quantity = dl.Quantity,
                    VatValue = dl.VatValue,
                    Invoice = this,
                    VatRate = dl.VatRate,
                    WarehouseReleaseLine = warehouseRelease?.Lines.First(l => l.Ordinal == dl.WarehouseReleaseLine!.Ordinal),
                    SalesOrderLine = salesOrder.Lines.FirstOrDefault(l => l.Ordinal == dl.SalesOrderLine?.Ordinal),
                }));
            }

            _salesOrders.AddRange(invoiceData.SalesOrders.Select(so => new SalesOrder(so)));
        }

        public Invoice(ISalesOrderData salesOrderData)
        {
            Number = $"I/{salesOrderData.Number}";
            CustomerId = salesOrderData.CustomerId;
            State = InvoiceState.Draft;
            Type = salesOrderData.AutoInvoice ? InvoiceType.Automatic : InvoiceType.Periodic;
            _salesOrders.Add(new SalesOrder(salesOrderData));
        }

        public string Number { get; protected set; }

        public decimal NetValue { get; protected set; }

        public decimal VatValue { get; protected set; }

        public decimal GrossValue { get; protected set; }

        public Guid CustomerId { get; protected set; }

        public InvoiceState State { get; protected set; }

        public InvoiceType Type { get; protected set; }

        public IEnumerable<IInvoiceLine> Lines => _lines;
        IEnumerable<IDocumentLine> IDocument.Lines => Lines;

        public IEnumerable<IVatSum> VatSums => _vatSums;

        public IEnumerable<SalesOrder> SalesOrders => _salesOrders;
        IEnumerable<ISalesOrderData> IInvoiceData.SalesOrders => _salesOrders;

        public abstract bool IsReadyToComplete();

        public abstract void Complete();

        public virtual void UpdateLines(IWarehouseReleaseData warehouseReleaseData)
        {
            if(State != InvoiceState.Draft)
            {
                throw new BusinessRuleException("Only drafts can be modified");
            }

            var salesOrder = _salesOrders.First(so => so.Number == warehouseReleaseData.SalesOrderNumber);
            var warehouseRelease = salesOrder.WarehouseReleases.FirstOrDefault(wr => wr.Number == warehouseReleaseData.Number);
            if(warehouseRelease is null)
            {
                warehouseRelease = salesOrder.WarehouseReleaseIssued(warehouseReleaseData);
            }
            else
            {
                warehouseRelease.Update(warehouseReleaseData);
            }

            List<int> existingOrdinals = [];
            
            foreach(var line in _lines.Where(l => l.WarehouseReleaseLine?.WarehouseRelease.Number == warehouseRelease.Number)
                .ToArray())
            {
                var newLine = warehouseRelease.Lines.FirstOrDefault(l => l.Ordinal  == line.Ordinal);
                if (newLine is null)
                {
                    _lines.Remove(line);
                }
                else 
                { 
                    existingOrdinals.Add(line.Ordinal);
                    UpdateLine(line, newLine);
                }
            }

            var i = 1;
            var maxOrdinal = _lines.Any() ? _lines.Max(l => l.Ordinal) : 0;
            _lines.AddRange(warehouseRelease.Lines
                .Where(l => !existingOrdinals.Contains(l.Ordinal))
                .Select(l => new InvoiceLine
                {
                    Invoice = this,
                    Ordinal = maxOrdinal + i++,
                    GrossValue = l.GrossValue,
                    NetValue = l.NetValue,
                    ProductId = l.ProductId,
                    Quantity = l.Quantity,
                    VatRate = l.VatRate,
                    VatValue = l.VatValue,
                    WarehouseReleaseLine = l,
                    SalesOrderLine = salesOrder.Lines.FirstOrDefault(sol => sol.Ordinal == l.SalesOrderLineOrdinal)
                }));

            UpdateTotals();
        }

        protected void UpdateTotals()
        {
            NetValue = _lines.Sum(l => l.NetValue);
            VatValue = _lines.Sum(l => l.VatValue);
            GrossValue = _lines.Sum(l => l.GrossValue);

            UpdateVatSums();
        }

        protected void UpdateVatSums()
        {
            List<VatRate> existingVatRates = [];
            foreach (var group in _lines.GroupBy(l => l.VatRate)) 
            {
                var vatSum = _vatSums.FirstOrDefault(vs => vs.VatRate == group.Key);
                if (vatSum == null) 
                {
                    vatSum = new VatSum
                    {
                        VatRate = group.Key,
                        GrossValue = group.Sum(l => l.GrossValue),
                        NetValue = group.Sum(l => l.NetValue),
                        VatValue = group.Sum(l => l.VatValue)
                    };
                    _vatSums.Add(vatSum);
                }
                else
                {
                    vatSum.NetValue = group.Sum(l => l.NetValue);
                    vatSum.GrossValue = group.Sum(l => l.GrossValue);
                    vatSum.VatValue = group.Sum(l => l.VatValue);
                }

                existingVatRates.Add(group.Key);
            }

            foreach(var vatSum in _vatSums.Where(vs => !existingVatRates.Contains(vs.VatRate)).ToArray())
            {
                _vatSums.Remove(vatSum);
            }
        }

        protected void UpdateLine(InvoiceLine line, WarehouseReleaseLine newLine)
        {
            line.VatRate = newLine.VatRate;
            line.VatValue = newLine.VatValue;
            line.NetValue = newLine.NetValue;
            line.ProductId = newLine.ProductId;
            line.Quantity = newLine.Quantity;
            line.GrossValue = newLine.GrossValue;
        }

        public void UpdateCollections(IInvoiceData entityData)
        {
            throw new NotImplementedException();
        }

        public void UpdateEntity(IInvoiceData entityData)
        {
            Number = entityData.Number;
            State = entityData.State;
            NetValue = entityData.NetValue;
            VatValue = entityData.VatValue;
            GrossValue = entityData.GrossValue;
        }
    }
}
