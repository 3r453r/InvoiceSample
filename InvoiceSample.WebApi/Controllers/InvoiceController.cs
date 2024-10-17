using InvoiceSample.Application.Services.Invoice;
using InvoiceSample.Domain.InvoiceAggregate;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceSample.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        public InvoiceController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [HttpGet]
        [Route("{invoiceNumber}")]
        public async Task<IActionResult> GetInvoice([FromRoute] string invoiceNumber) 
        { 
            var invoiceData = await _invoiceService.GetInvoice(invoiceNumber);
            return invoiceData is null ? NotFound() : Ok(invoiceData);
        }

        [HttpPost]
        [Route("endPeriod/{customerId}")]
        public async Task<IActionResult> EndPeriod([FromRoute] Guid customerId)
        {
            var invoiceData = await _invoiceService.EndPeriod(customerId);
            await _invoiceService.SaveChanges();
            return Ok(invoiceData);
        }
    }
}
