using AutoMapper;
using InvoiceSample.Application.Persistence;
using InvoiceSample.DataDrivenEntity.Extensions;
using InvoiceSample.Domain.InvoiceAggregate;
using Microsoft.EntityFrameworkCore;
using InvoiceTable = InvoiceSample.Persistence.Tables.Invoice;

namespace InvoiceSample.Persistence.ApplicationImplementtion.Repositiories
{
    public class InvoiceRepository : EFRepository<InvoiceTable, Guid, IInvoiceData>
        , IInvoiceRepository
    {
        private readonly InvoiceSampleDbContext _context;
        private readonly IMapper _mapper;

        public InvoiceRepository(InvoiceSampleDbContext dbContext, IMapper mapper) : base(dbContext)
        {
            _context = dbContext;
            _mapper = mapper;
        }

        private IQueryable<InvoiceTable> InvoicesWithAllDependencies => _context.Invoices
            .Include(i => i.SalesOrders).ThenInclude(so => so.Lines)
            .Include(i => i.Lines)
            .Include(i => i.Lines).ThenInclude(l => l.WarehouseReleaseLine).ThenInclude(wrl => wrl!.WarehouseMovement)
            .Include(i => i.Lines).ThenInclude(l => l.SalesOrderLine).ThenInclude(sol => sol!.SalesOrder)
            .Include(i => i.VatSums)
            .AsSplitQuery();

        public async Task Add(IInvoiceData entity)
        {
            await AddOrUpdateEntity(entity, _mapper);
        }

        protected override InvoiceTable CreateTableEntity() => new InvoiceTable();

        public async Task Delete(IInvoiceData entity)
        {
            var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.Number == entity.Number);
            if (invoice is not null)
            {
                _context.Invoices.Remove(invoice);
            }
        }

        public override async Task<InvoiceTable?> FindByDataKey(Guid key)
        {
            return await InvoicesWithAllDependencies.FirstOrDefaultAsync(i => i.Id == key);
        }

        public async Task<IInvoiceData?> GetByNumber(string number)
        {
            return await InvoicesWithAllDependencies.FirstOrDefaultAsync(i => i.Number == number);
        }

        public async Task<IEnumerable<IInvoiceData>> GetBySalesOrderNumber(string salesOrderNumber)
        {
            return await InvoicesWithAllDependencies
                .Where(i => i.SalesOrders.Any(so => so.Number == salesOrderNumber))
                .ToListAsync();
        }

        public async Task<IInvoiceData?> GetDraftBySalesOrderNumber(string salesOrderNumber)
        {
            return await InvoicesWithAllDependencies
                .SingleOrDefaultAsync(i => i.SalesOrders.Any(so => so.Number == salesOrderNumber)
                && i.State == InvoiceState.Draft);
        }

        public async Task<IInvoiceData?> GetPeriodicDraftByCustomer(Guid customerId)
        {
            return await InvoicesWithAllDependencies
                .FirstOrDefaultAsync(i => i.CustomerId == customerId
                    && i.Type == InvoiceType.Periodic);
        }

        public async Task<bool> SalesOrderInvoiced(string salesOrderNumber)
        {
            return await _context.Invoices
                .AnyAsync(i => i.State != InvoiceState.Draft
                && i.SalesOrders.Any(so => so.Number == salesOrderNumber));
        }

        public async Task Update(IInvoiceData entity)
        {
            await AddOrUpdateEntity(entity, _mapper);
        }
    }
}