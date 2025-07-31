using InvoiceSample.WebApi; // For Program/EntryPoint
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using InvoiceSample.WebApi.Dtos; // For request DTOs
using InvoiceSample.Domain; // For VatRate etc.
using InvoiceSample.Persistence; // For DbContext
using Microsoft.EntityFrameworkCore;
using InvoiceSample.Domain.InvoiceAggregate; // For IInvoiceData
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection.Extensions;
using InvoiceSample.Domain.SalesOrderAggregate;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using InvoiceSample.Application.EventBus;
using InvoiceSample.Application.Persistence;
using InvoiceSample.Application.Services.Invoice;
using InvoiceSample.DataDrivenEntity.Extensions;
using InvoiceSample.Persistence.ApplicationImplementtion;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using InvoiceSample.DataDrivenEntity;
using System.Text.Json.Serialization;

namespace InvoiceSample.WebApi.Tests
{
        public class InvoiceApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>> // Use Program as EntryPoint
        {
            private readonly WebApplicationFactory<Program> _factory;
            private readonly HttpClient _client;

        public InvoiceApiIntegrationTests(WebApplicationFactory<Program> factory)
        {
            // Create a completely new WebHostBuilder
            _factory = factory;

            var hostBuilder = new WebHostBuilder()
                .UseEnvironment("Testing")
                .ConfigureAppConfiguration(config =>
                {
                    config.AddJsonFile("appsettings.json", optional: true);
                    config.AddJsonFile("appsettings.Testing.json", optional: true);
                    config.AddInMemoryCollection(new Dictionary<string, string>
                    {
                    {"ConnectionStrings:SqlServer", "TestConnectionString"}
                    });
                })
                .ConfigureServices(services =>
                {
                    services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
        // You might also want to set a max depth if needed
        options.JsonSerializerOptions.MaxDepth = 64; // Increase from default of 32
    }).AddApplicationPart(typeof(Program).Assembly);

                    // Add in-memory database
                    services.AddDbContext<InvoiceSampleDbContext>(opt =>
                        opt.UseInMemoryDatabase($"InMemoryDb_{Guid.NewGuid()}"));

                    // Add other required services
                    services.AddScoped<IInvoiceSampleUnitOfWork, InvoiceSampleUnitOfWork>();
                    services.AddSingleton<IEventBus, MockEventBus>();
                    services.AddScoped<IInvoiceService, InvoiceService>();

                    // Add AutoMapper with the same settings as in Program.cs
                    services.AddAutoMapper(cfg => {
                        cfg.GloballyIgnoreProperties(cfg =>
                        {
                            cfg.Ignore<IDataDrivenEntityBase>()
                            .IgnoreCollections<IDataDrivenEntityBase>()
                            .Ignore<IEntityData>()
                            .IgnoreCollections<IEntityData>();
                        });
                    },
                    typeof(InvoiceSample.Domain.MappingProfiles.InvoiceMappingProfile).Assembly,
                    typeof(InvoiceSample.Persistence.MappingProfiles.InvoiceMappingProfile).Assembly);
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers();
                    });
                });

            var testServer = new TestServer(hostBuilder);
            _client = testServer.CreateClient();
        }

        private async Task<IInvoiceData?> GetInvoiceFromDbAsync(Guid invoiceId)
            {
                using var scope = _factory.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<InvoiceSampleDbContext>();
                // Use persistence entities to query, then map or return as needed
                // NOTE: This requires loading related data similar to the repository
                var persistenceInvoice = await dbContext.Invoices
                    .Include(i => i.SalesOrders).ThenInclude(so => so.Lines)
                    .Include(i => i.Lines) //.ThenInclude(...) // Add necessary includes
                    .Include(i => i.VatSums)
                    .AsNoTracking() // Good practice for assertions
                    .FirstOrDefaultAsync(i => i.Id == invoiceId);

                // You might need to map this back to an IInvoiceData or assert directly on persistenceInvoice
                return persistenceInvoice; // Returning persistence type for simplicity here
            }

            private async Task<Persistence.Tables.SalesOrder?> GetSalesOrderFromDbAsync(string soNumber)
            {
                using var scope = _factory.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<InvoiceSampleDbContext>();
                return await dbContext.SalesOrders
                   .Include(so => so.Lines)
                   .Include(so => so.WarehouseReleases) // Include WRs if needed
                   .AsNoTracking()
                   .FirstOrDefaultAsync(so => so.Number == soNumber);
            }


            [Fact]
            public async Task PostSalesOrder_Automatic_ShouldCreateInvoiceAndSalesOrder()
            {
                // Arrange
                var customerId = Guid.NewGuid();
                var productId = Guid.NewGuid();
                var soNumber = "SO-INT-001";
                var request = new SalesOrderRequested
                {
                    Number = soNumber,
                    CustomerId = customerId,
                    AutoInvoice = true, // -> Automatic Invoice
                    Lines = new List<Dtos.SalesOrderLine>
                {
                    new Dtos.SalesOrderLine { Ordinal = 1, ProductId = productId, Quantity = 2, NetValue = 50, VatValue = 11.5m, GrossValue = 61.5m, VatRate = VatRate.TwentyThree, IsService = false }
                }
                };

                // Act
                var response = await _client.PostAsJsonAsync("/salesorder", request);

                // Assert
                response.EnsureSuccessStatusCode(); // Check for 2xx status code
                var createdInvoice = await response.Content.ReadFromJsonAsync<IInvoiceData>(); // Assuming API returns the created invoice (adjust type if needed)

                Assert.NotNull(createdInvoice);
                Assert.Equal($"I/{soNumber}", createdInvoice.Number); // Default numbering scheme
                Assert.Equal(customerId, createdInvoice.CustomerId);
                Assert.Equal(InvoiceType.Automatic, createdInvoice.Type); // Verify type
                Assert.Equal(InvoiceState.Draft, createdInvoice.State); // Should be draft initially

                // Verify Database state
                using var scope = _factory.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<InvoiceSampleDbContext>();

                var dbInvoice = await dbContext.Invoices
                                         .Include(i => i.SalesOrders)
                                         .FirstOrDefaultAsync(i => i.Number == createdInvoice.Number);
                Assert.NotNull(dbInvoice);
                Assert.Single(dbInvoice.SalesOrders);
                Assert.Equal(soNumber, dbInvoice.SalesOrders[0].Number);

                var dbSalesOrder = await dbContext.SalesOrders
                                            .Include(so => so.Lines)
                                            .FirstOrDefaultAsync(so => so.Number == soNumber);
                Assert.NotNull(dbSalesOrder);
                Assert.Single(dbSalesOrder.Lines);
                Assert.Equal(productId, dbSalesOrder.Lines[0].ProductId);
            }

            [Fact]
            public async Task PostWarehouseRelease_ForAutomaticInvoice_ShouldUpdateInvoiceLines()
            {
                // Arrange: First create an automatic SO/Invoice
                var customerId = Guid.NewGuid();
                var productId = Guid.NewGuid();
                var soNumber = "SO-INT-002";
                var initialSoRequest = new SalesOrderRequested
                {
                    Number = soNumber,
                    CustomerId = customerId,
                    AutoInvoice = true,
                    Lines = new List<Dtos.SalesOrderLine>
                {
                    new Dtos.SalesOrderLine { Ordinal = 1, ProductId = productId, Quantity = 5, NetValue = 100, VatValue = 23, GrossValue = 123, VatRate = VatRate.TwentyThree, IsService = false }
                }
                };
                var soResponse = await _client.PostAsJsonAsync("/salesorder", initialSoRequest);
                soResponse.EnsureSuccessStatusCode();
                var createdInvoiceHeader = await soResponse.Content.ReadFromJsonAsync<IInvoiceData>();
                Assert.NotNull(createdInvoiceHeader);

                // Now create the Warehouse Release DTO
                var wrNumber = "WR-INT-001";
                var wrRequest = new WarehouseReleaseRequested
                {
                    Number = wrNumber,
                    SalesOrderNumber = soNumber,
                    CustomerId = customerId,
                    Lines = new List<WarehouseReleaseRequested.WarehouseReleaseLine>
                {
                     // Release 3 out of 5
                    new WarehouseReleaseRequested.WarehouseReleaseLine { Ordinal = 1, SalesOrderLineOrdinal = 1, ProductId = productId, Quantity = 3, NetValue = 60, VatValue = 13.8m, GrossValue = 73.8m, VatRate = VatRate.TwentyThree }
                }
                };

                // Act
                var wrResponse = await _client.PostAsJsonAsync("/warehousemovement/warehouseRelease", wrRequest);

                // Assert
                wrResponse.EnsureSuccessStatusCode();
                var updatedInvoice = await wrResponse.Content.ReadFromJsonAsync<IInvoiceData>(); // API returns updated invoice

                Assert.NotNull(updatedInvoice);
                Assert.Equal(createdInvoiceHeader.Id, updatedInvoice.Id); // Should be the same invoice
                Assert.Equal(InvoiceState.Draft, updatedInvoice.State); // Still draft (assuming 3/5 released is not 'complete')
                                                                        // Values should reflect the *released* amount
                Assert.Equal(60m, updatedInvoice.NetValue);
                Assert.Equal(13.8m, updatedInvoice.VatValue);
                Assert.Equal(73.8m, updatedInvoice.GrossValue);

                // Verify Database state
                var dbInvoice = await GetInvoiceFromDbAsync(updatedInvoice.Id);
                Assert.NotNull(dbInvoice);
                Assert.Single(dbInvoice.Lines); // Should have one line corresponding to the WR line
                Assert.Equal(60m, dbInvoice.NetValue);
                var dbInvoiceLine = dbInvoice.Lines.First();
                Assert.Equal(productId, dbInvoiceLine.ProductId);
                Assert.Equal(3, dbInvoiceLine.Quantity); // Quantity from WR
                Assert.NotNull(dbInvoiceLine.WarehouseReleaseLine); // Verify link to WR Line
            }

            [Fact]
            public async Task PostWarehouseRelease_CompletingAutomaticInvoice_ShouldSetStateAndPublishEvent()
            {
                // Arrange: Create SO/Invoice
                var customerId = Guid.NewGuid();
                var productId = Guid.NewGuid();
                var soNumber = "SO-INT-003";
                var initialSoRequest = new SalesOrderRequested
                { /* ... include IsService=true line ... */
                    Number = soNumber,
                    AutoInvoice = true,
                    CustomerId = customerId,
                    Lines = { new Dtos.SalesOrderLine { Ordinal = 1, IsService = true, ProductId = productId, Quantity = 1, NetValue = 10, VatValue = 2.3m, GrossValue = 12.3m, VatRate = VatRate.TwentyThree } }
                };
                (await _client.PostAsJsonAsync("/salesorder", initialSoRequest)).EnsureSuccessStatusCode();

                // Arrange: Warehouse Release (assuming it completes the order - might need more complex setup)
                var wrNumber = "WR-INT-002";
                var wrRequest = new WarehouseReleaseRequested { /* ... data ... */ Number = wrNumber, SalesOrderNumber = soNumber, CustomerId = customerId, Lines = { } }; // Add lines if needed to complete

                //TODO: Mock IEventBus to verify PublishIntegrationEvent<PrintInvoiceRequested> call
                // This requires more advanced WebApplicationFactory setup to replace services.
                // For now, we'll just check the state change.

                // Act
                var wrResponse = await _client.PostAsJsonAsync("/warehousemovement/warehouseRelease", wrRequest);

                // Assert
                wrResponse.EnsureSuccessStatusCode();
                var finalInvoice = await wrResponse.Content.ReadFromJsonAsync<IInvoiceData>();

                Assert.NotNull(finalInvoice);
                // State depends on IsReadyToComplete logic - assume it becomes ReadyToInvoice
                Assert.Equal(InvoiceState.ReadyToInvoice, finalInvoice.State);

                // Verify DB
                var dbInvoice = await GetInvoiceFromDbAsync(finalInvoice.Id);
                Assert.NotNull(dbInvoice);
                Assert.Equal(InvoiceState.ReadyToInvoice, dbInvoice.State);
                // Check if service lines were added
                Assert.Contains(dbInvoice.Lines, line => line.SalesOrderLine != null);
            }


            [Fact]
            public async Task PostEndPeriod_ForPeriodicInvoice_ShouldCompleteAndPublishEvent()
            {
                // Arrange: Create a Periodic SO/Invoice
                var customerId = Guid.NewGuid();
                var productId = Guid.NewGuid();
                var soNumber = "SO-INT-004";
                var initialSoRequest = new SalesOrderRequested
                {
                    Number = soNumber,
                    CustomerId = customerId,
                    AutoInvoice = false, // Periodic
                    Lines = { new Dtos.SalesOrderLine { Ordinal = 1, IsService = true, ProductId = productId, Quantity = 1, NetValue = 50, VatValue = 11.5m, GrossValue = 61.5m, VatRate = VatRate.TwentyThree } }
                };
                var soResponse = await _client.PostAsJsonAsync("/salesorder", initialSoRequest);
                soResponse.EnsureSuccessStatusCode();
                var createdInvoiceHeader = await soResponse.Content.ReadFromJsonAsync<PeriodicInvoice>();
                Assert.NotNull(createdInvoiceHeader);
                Assert.Equal(InvoiceType.Periodic, createdInvoiceHeader.Type);

                // TODO: Mock IEventBus if needed

                // Act
                var endPeriodResponse = await _client.PostAsync($"/invoice/endPeriod/{customerId}", null); // No body needed

                // Assert
                endPeriodResponse.EnsureSuccessStatusCode();
                var finalInvoice = await endPeriodResponse.Content.ReadFromJsonAsync<IInvoiceData>();

                Assert.NotNull(finalInvoice);
                Assert.Equal(createdInvoiceHeader.Id, finalInvoice.Id);
                Assert.Equal(InvoiceState.ReadyToInvoice, finalInvoice.State); // Should complete
                Assert.Equal(50m, finalInvoice.NetValue); // Should include service line value

                // Verify DB
                var dbInvoice = await GetInvoiceFromDbAsync(finalInvoice.Id);
                Assert.NotNull(dbInvoice);
                Assert.Equal(InvoiceState.ReadyToInvoice, dbInvoice.State);
                Assert.Single(dbInvoice.Lines); // Service line should be added
                Assert.Equal(50m, dbInvoice.NetValue);

            }

            // Add tests for:
            // - GET /invoice/{invoiceNumber}
            // - Posting SalesOrder with AutoInvoice = false (Periodic)
            // - Posting WarehouseRelease for Periodic (should update lines but not complete)
            // - EndPeriod when no draft exists (should handle gracefully)
            // - Error cases (e.g., WR for non-existent SO)

        }
    }