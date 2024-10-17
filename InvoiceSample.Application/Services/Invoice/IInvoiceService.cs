using InvoiceSample.Domain.InvoiceAggregate;
using InvoiceSample.Domain.SalesOrderAggregate;
using InvoiceSample.Domain.WarehouseReleaseAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Application.Services.Invoice
{
    public interface IInvoiceService
    {
        Task<IInvoiceData?> GetInvoice(string number);
        Task<IInvoiceData> AddOrUpdateInvoice(ISalesOrderData salesOrderData);
        Task<IInvoiceData> UpdateInvoice(IWarehouseReleaseData warehouseReleaseData);
        Task<IInvoiceData?> EndPeriod(Guid customerId);

        Task SaveChanges();
    }
}
