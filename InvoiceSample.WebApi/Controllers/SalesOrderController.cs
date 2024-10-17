using InvoiceSample.Application.Services.Invoice;
using InvoiceSample.WebApi.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceSample.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SalesOrderController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        public SalesOrderController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }
        
        [HttpPost]
        public async Task<IActionResult> SalesOrderIssued([FromBody] SalesOrderRequested request)
        {
            var invoiceData = await _invoiceService.AddOrUpdateInvoice(request);
            await _invoiceService.SaveChanges();
            return Ok(invoiceData);
        }
    }
}
