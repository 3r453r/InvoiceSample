using InvoiceSample.Application.Persistence;
using InvoiceSample.Domain.InvoiceAggregate;
using InvoiceSample.Domain.SalesOrderAggregate;
using InvoiceSample.Domain.WarehouseReleaseAggregate;
using InvoiceSample.Persistence.Tables;
using Microsoft.EntityFrameworkCore;
using InvoiceTable = InvoiceSample.Persistence.Tables.Invoice;

namespace InvoiceSample.Persistence.ApplicationImplementtion.Repositiories
{
    internal class InvoiceRepository : IInvoiceRepository
    {
        private readonly InvoiceSampleDbContext _dbContext;

        public InvoiceRepository(InvoiceSampleDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task Add(IInvoiceData entity)
        {
            var invoice = new InvoiceTable { 
                Number = entity.Number,
                State = entity.State,
                CustomerId = entity.CustomerId,
                GrossValue = entity.GrossValue,
                NetValue = entity.NetValue,
                Type = entity.Type,
                VatValue = entity.VatValue,
            };

            AddOrUpdateSalesOrders(invoice, entity.SalesOrders);
            AddOrUpdateInvoiceLines(invoice, entity.Lines);
            AddOrUpdateVatSums(invoice.VatSums, entity.VatSums, invoice);

            await _dbContext.Invoices.AddAsync(invoice);
        }

        public async Task Update(IInvoiceData entity)
        {
            var invoice = await InvoicesWithAllDependencies
                .FirstAsync(i => i.Number == entity.Number);

            invoice.Number = entity.Number;
            invoice.State = entity.State;
            invoice.CustomerId = entity.CustomerId;
            invoice.GrossValue = entity.GrossValue;
            invoice.NetValue = entity.NetValue;
            invoice.Type = entity.Type;
            invoice.VatValue = entity.VatValue;

            AddOrUpdateSalesOrders(invoice, entity.SalesOrders);
            AddOrUpdateInvoiceLines(invoice, entity.Lines);
            AddOrUpdateVatSums(invoice.VatSums, entity.VatSums, invoice);
        }

        public async Task Delete(IInvoiceData entity)
        {
            var invoice = await _dbContext.Invoices.FirstOrDefaultAsync(i => i.Number == entity.Number);
            if (invoice is not null)
            {
                _dbContext.Invoices.Remove(invoice);
            }
        }

        public async Task<IInvoiceData?> GetByNumber(string number)
        {
            return await InvoicesWithAllDependencies.FirstOrDefaultAsync(i => i.Number == number);
        }

        public async Task<IEnumerable<IInvoiceData>> GetBySalesOrderNumber(string salesOrderNumber)
        {
            return await InvoicesWithAllDependencies
                .Where(i => i.SalesOrders.Any(so => so.Number == salesOrderNumber))
                .ToArrayAsync();
        }

        public async Task<IInvoiceData?> GetDraftBySalesOrderNumber(string salesOrderNumber)
        {
            return await InvoicesWithAllDependencies
                .FirstOrDefaultAsync(i => i.State == InvoiceState.Draft
                    && i.SalesOrders.Any(so => so.Number == salesOrderNumber));
        }

        public async Task<IInvoiceData?> GetPeriodicDraftByCustomer(Guid customerId)
        {
            return await InvoicesWithAllDependencies
                .FirstOrDefaultAsync(i => i.State == InvoiceState.Draft
                    && i.CustomerId == customerId
                    && i.SalesOrders.Any(so => !so.AutoInvoice));
        }

        public async Task<bool> SalesOrderInvoiced(string salesOrderNumber)
        {
            InvoiceState[] invoicedStates = [InvoiceState.ReadyToInvoice, InvoiceState.Invoiced];

            return await _dbContext.Invoices.AnyAsync(i =>
                i.SalesOrders.Any(so => so.Number == salesOrderNumber)
                && invoicedStates.Contains(i.State));
        }

        private void AddOrUpdateVatSums(List<InvoiceVatSum> vatSums, IEnumerable<IVatSum> vatSumsData, InvoiceTable invoice)
        {
            foreach(var vatSumData in vatSumsData)
            {
                var vatSum = vatSums.FirstOrDefault(s => s.VatRate == vatSumData.VatRate);
                if (vatSum == null) 
                {
                    vatSum = new InvoiceVatSum
                    {
                        Invoice = invoice,
                        VatRate = vatSumData.VatRate,
                        GrossValue = vatSumData.GrossValue,
                        NetValue = vatSumData.NetValue,
                        VatValue = vatSumData.VatValue,
                    };

                    vatSums.Add(vatSum);
                }
                else
                {
                    vatSum.GrossValue = vatSumData.GrossValue;
                    vatSum.NetValue = vatSumData.NetValue;
                    vatSum.VatValue = vatSumData.VatValue;
                }
            }
        }

        private void AddOrUpdateInvoiceLines(InvoiceTable invoice, IEnumerable<IInvoiceLine> lines)
        {
            List<int> existingOrdinals = [];
            foreach (var lineData in lines)
            { 
                var invoiceLine = invoice.Lines.FirstOrDefault(l => l.Ordinal == lineData.Ordinal);
                if (invoiceLine != null) 
                { 
                    invoiceLine.NetValue = lineData.NetValue;
                    invoiceLine.VatValue = lineData.VatValue;
                    invoiceLine.GrossValue = lineData.GrossValue;
                    invoiceLine.ProductId = lineData.ProductId;
                    invoiceLine.VatRate = lineData.VatRate;

                    if(!(lineData.SalesOrderLine is null && lineData.SalesOrderLine is null)
                        || !(invoiceLine.SalesOrderLine?.IsEqual(lineData.SalesOrderLine) ?? false))
                    {
                        invoiceLine.SalesOrderLine = FindSalesOrderLine(invoice.SalesOrders, lineData.SalesOrderLine);
                    }

                    if ((lineData.WarehouseReleaseLine is null && lineData.WarehouseReleaseLine is null)
                        || !(invoiceLine.WarehouseReleaseLine?.IsEqual(lineData.WarehouseReleaseLine) ?? false))
                    {
                        invoiceLine.WarehouseReleaseLine = FindWarehouseReleaseLine(
                            invoice.SalesOrders.SelectMany(so => so.WarehouseReleases)
                            , lineData.WarehouseReleaseLine);
                    }
                    
                    existingOrdinals.Add(invoiceLine.Ordinal);
                }
                else
                {
                    var newInvoiceLine = new Tables.InvoiceLine
                    {
                        Invoice = invoice,
                        Ordinal = lineData.Ordinal,
                        VatValue = lineData.VatValue,
                        Quantity = lineData.Quantity,
                        GrossValue = lineData.GrossValue,
                        NetValue = lineData.NetValue,
                        ProductId = lineData.ProductId,
                        VatRate = lineData.VatRate,
                        SalesOrderLine = FindSalesOrderLine(invoice.SalesOrders, lineData.SalesOrderLine),
                        WarehouseReleaseLine = FindWarehouseReleaseLine(invoice.SalesOrders.SelectMany(so => so.WarehouseReleases)
                            , lineData.WarehouseReleaseLine),
                    };

                    invoice.Lines.Add(newInvoiceLine);
                    existingOrdinals.Add(newInvoiceLine.Ordinal);
                }
            }

            foreach(var line in invoice.Lines.Where(l => !existingOrdinals.Contains(l.Ordinal)).ToArray())
            {
                _dbContext.InvoiceLines.Remove(line);
            }
        }

        private WarehouseMovementLine? FindWarehouseReleaseLine(IEnumerable<WarehouseMovement> warehouseReleases, IWarehouseReleaseLine? warehouseReleaseLine)
        {
            return warehouseReleases
                .FirstOrDefault(wr => wr.Number == warehouseReleaseLine?.WarehouseRelease.Number)
                ?.Lines.FirstOrDefault(wrl => wrl.Ordinal == warehouseReleaseLine?.Ordinal);
        }

        private Tables.SalesOrderLine? FindSalesOrderLine(List<Tables.SalesOrder> salesOrders, ISalesOrderLine? salesOrderLine)
        {
            return salesOrders
                .FirstOrDefault(so => so.Number == salesOrderLine?.SalesOrder.Number)
                ?.Lines.FirstOrDefault(sol => sol.Ordinal == salesOrderLine?.Ordinal);
        }

        private void AddOrUpdateSalesOrders(InvoiceTable invoice, IEnumerable<ISalesOrderData> salesOrders)
        {
            foreach (var salesOrderData in salesOrders)
            {
                var salesOrder = invoice.SalesOrders.FirstOrDefault(so => so.Number == salesOrderData.Number);
                if(salesOrder is null)
                {
                    salesOrder = new Tables.SalesOrder
                    {
                        Number = salesOrderData.Number,
                        AutoInvoice = salesOrderData.AutoInvoice,
                        CustomerId = salesOrderData.CustomerId,
                    };

                    invoice.SalesOrders.Add(salesOrder);
                }
                AddOrUpdateSalesOrderLines(salesOrder, salesOrderData.Lines);
                AddOrUpdateWarehouseReleases(salesOrder.WarehouseReleases, salesOrderData.WarehouseReleases, salesOrder);
            }
        }

        private void AddOrUpdateWarehouseReleases(List<WarehouseMovement> warehouseReleases
            , IEnumerable<IWarehouseReleaseData> warehouseReleasesData
            , Tables.SalesOrder salesOrder)
        {
            List<int> existingOrdinals = [];
            foreach (var warehouseReleaseData in warehouseReleasesData)
            {
                var warehouseRelease = warehouseReleases.FirstOrDefault(wr => wr.Number == warehouseReleaseData.Number);

                if(warehouseRelease is null)
                {
                    warehouseRelease = new WarehouseMovement
                    {
                        Number = warehouseReleaseData.Number,
                        SalesOrder = salesOrder,
                        Type = WarehouseMovementType.WarehouseRelease,                       
                    };
                    salesOrder.WarehouseReleases.Add(warehouseRelease);
                }
                
                AddOrUpdateWarehouseMovementLines(warehouseRelease.Lines, warehouseReleaseData.Lines, warehouseRelease, salesOrder);
            }
        }

        private void AddOrUpdateWarehouseMovementLines(List<WarehouseMovementLine> lines
            , IEnumerable<IWarehouseReleaseLine> linesData
            , WarehouseMovement warehouseMovement
            , Tables.SalesOrder salesOrder)
        {
            List<int> existingOrdinals = [];
            foreach (var lineData in linesData) 
            {
                var line = lines.FirstOrDefault(l => l.Ordinal == lineData.Ordinal);
                if(line is null)
                {
                    line = new WarehouseMovementLine
                    {
                        WarehouseMovement = warehouseMovement,
                        Ordinal = lineData.Ordinal,
                        GrossValue = lineData.GrossValue,
                        NetValue = lineData.NetValue,
                        VatValue = lineData.VatValue,
                        ProductId = lineData.ProductId,
                        Quantity = lineData.Quantity,
                        VatRate = lineData.VatRate,
                        SalesOrderLine = salesOrder.Lines.FirstOrDefault(l => l.Ordinal == lineData.SalesOrderLineOrdinal)
                    };

                    warehouseMovement.Lines.Add(line);
                }
                else
                {
                    line.GrossValue = lineData.GrossValue;
                    line.NetValue = lineData.NetValue;
                    line.VatValue = lineData.VatValue;
                    line.ProductId = lineData.ProductId;
                    line.Quantity = lineData.Quantity;
                    line.VatRate = lineData.VatRate;

                    if (line.SalesOrderLine?.Ordinal != line.SalesOrderLineOrdinal)
                    {
                        line.SalesOrderLine = salesOrder.Lines.FirstOrDefault(l => l.Ordinal == lineData.SalesOrderLineOrdinal);
                    }
                }

                existingOrdinals.Add(line.Ordinal);
            }

            foreach (var line in lines.Where(l => !existingOrdinals.Contains(l.Ordinal)).ToArray()) 
            {
                _dbContext.WarehouseMovementLines.Remove(line);
            }
        }

        private void AddOrUpdateSalesOrderLines(Tables.SalesOrder salesOrder, IEnumerable<ISalesOrderLine> lines)
        {
            List<int> existingOrdinals = [];
            foreach(var lineData in lines)
            {
                var salesOrderLine = salesOrder.Lines.FirstOrDefault(l => l.Ordinal ==  lineData.Ordinal);
                if(salesOrderLine is null)
                {
                    var newSalesOrderLine = new Tables.SalesOrderLine
                    {
                        SalesOrder = salesOrder,
                        Ordinal = lineData.Ordinal,
                        GrossValue = lineData.GrossValue,
                        IsService = lineData.IsService,
                        NetValue = lineData.NetValue,
                        ProductId = lineData.ProductId,
                        Quantity = lineData.Quantity,
                        VatRate = lineData.VatRate,
                        VatValue = lineData.VatValue,
                    };

                    salesOrder.Lines.Add(newSalesOrderLine);
                    existingOrdinals.Add(newSalesOrderLine.Ordinal);
                }
                else
                {
                    salesOrderLine.GrossValue = lineData.GrossValue;
                    salesOrderLine.NetValue = lineData.NetValue;
                    salesOrderLine.VatValue = lineData.VatValue;
                    salesOrderLine.IsService = lineData.IsService;
                    salesOrderLine.ProductId = lineData.ProductId;
                    salesOrderLine.Quantity = lineData.Quantity;
                    salesOrderLine.VatRate = lineData.VatRate;

                    existingOrdinals.Add(salesOrderLine.Ordinal);
                }
            }

            foreach (var line in salesOrder.Lines.Where(l => !existingOrdinals.Contains(l.Ordinal)).ToArray())
            {
                _dbContext.SalesOrderLines.Remove(line);
            }
        }

        private IQueryable<InvoiceTable> InvoicesWithAllDependencies => _dbContext.Invoices
            .Include(i => i.SalesOrders).ThenInclude(so => so.Lines)
            .Include(i => i.Lines)
            .Include(i => i.Lines).ThenInclude(l => l.WarehouseReleaseLine).ThenInclude(wrl => wrl!.WarehouseMovement)
            .Include(i => i.Lines).ThenInclude(l => l.SalesOrderLine).ThenInclude(sol => sol!.SalesOrder)
            .Include(i => i.VatSums)
            .AsSplitQuery();
    }
}