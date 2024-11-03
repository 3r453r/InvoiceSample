using AutoMapper;
using InvoiceSample.Application.Persistence;
using InvoiceSample.Persistence.ApplicationImplementtion.Repositiories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Persistence.ApplicationImplementtion
{
    public class InvoiceSampleUnitOfWork : IInvoiceSampleUnitOfWork
    {
        private readonly InvoiceSampleDbContext _dbContext;
        private readonly InvoiceRepository _invoiceRepository;

        public InvoiceSampleUnitOfWork(InvoiceSampleDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _invoiceRepository = new InvoiceRepository(dbContext, mapper);
        }

        public IInvoiceRepository InvoiceRepository => _invoiceRepository;

        public Task SaveChanges()
        {
            return _dbContext.SaveChangesAsync();
        }
    }
}
