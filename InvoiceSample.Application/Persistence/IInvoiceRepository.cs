using InvoiceSample.Domain.InvoiceAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Application.Persistence
{
    public interface IInvoiceRepository : IRepository<IInvoiceData>
    {
        Task<IInvoiceData?> GetDraftBySalesOrderNumber(string salesOrderNumber);

        Task<IEnumerable<IInvoiceData>> GetBySalesOrderNumber(string salesOrderNumber);

        Task<IInvoiceData?> GetPeriodicDraftByCustomer(Guid customerId);

        Task<bool> SalesOrderInvoiced(string salesOrderNumber);
    }
}
