using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Application.EventBus
{
    public class MockEventBus : IEventBus
    {
        private readonly ILogger<MockEventBus> _logger;

        public MockEventBus(ILogger<MockEventBus> logger)
        {
            _logger = logger;
        }

        public Task PublishDomainEvent<TEvent>(TEvent @event) where TEvent : IEvent
        {
            _logger.LogInformation("Publishing domain event {@event}", @event);
            return Task.CompletedTask;
        }

        public Task PublishIntegrationCommand<TEvent>(TEvent @event) where TEvent : IEvent
        {
            _logger.LogInformation("Publishing integration command {@event}", @event);
            return Task.CompletedTask;
        }

        public Task PublishIntegrationEvent<TEvent>(TEvent @event) where TEvent : IEvent
        {
            _logger.LogInformation("Publishing integration event {@event}", @event);
            return Task.CompletedTask;
        }
    }
}
