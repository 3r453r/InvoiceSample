using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Application.EventBus
{
    public interface IEventBus
    {
        Task PublishDomainEvent<TEvent>(TEvent @event) where TEvent: IEvent;

        Task PublishIntegrationEvent<TEvent>(TEvent @event) where TEvent : IEvent;

        Task PublishIntegrationCommand<TEvent>(TEvent @event) where TEvent : IEvent;
    }
}
