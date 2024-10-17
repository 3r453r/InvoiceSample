using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Application.EventBus
{
    public interface IEvent
    {
        Guid EventId { get; }
        Guid CorrelationId { get; }
    }
}
