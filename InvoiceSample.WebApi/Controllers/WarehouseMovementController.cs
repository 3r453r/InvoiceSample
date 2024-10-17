using InvoiceSample.Application.Services.Invoice;
using InvoiceSample.WebApi.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceSample.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WarehouseMovementController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        public WarehouseMovementController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [HttpPost]
        [Route("warehouseRelease")]
        public async Task<IActionResult> WarehouseReleaseIssued([FromBody] WarehouseReleaseRequested request)
        {
            var invoiceData = await _invoiceService.UpdateInvoice(request);
            await _invoiceService.SaveChanges();

            return Ok(invoiceData);
        }
    }
}
