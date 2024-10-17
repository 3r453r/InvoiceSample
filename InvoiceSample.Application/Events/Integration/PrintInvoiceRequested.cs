using InvoiceSample.Application.EventBus;
using InvoiceSample.Domain.InvoiceAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Application.Events.Integration
{
    public record PrintInvoiceRequested : IEvent
    {
        public Guid EventId { get; } = Guid.NewGuid();
        public Guid CorrelationId { get; set; } = Guid.NewGuid();
        public required IInvoiceData Invoice { get; init; }
    }
}
