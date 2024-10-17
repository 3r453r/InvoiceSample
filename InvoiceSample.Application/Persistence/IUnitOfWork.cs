using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Application.Persistence
{
    public interface IUnitOfWork
    {
        Task SaveChanges();
    }

    public interface IInvoiceSampleUnitOfWork : IUnitOfWork
    {
        IInvoiceRepository InvoiceRepository { get; }
    }
}
