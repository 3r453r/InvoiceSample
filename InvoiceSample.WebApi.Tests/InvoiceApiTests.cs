using FluentAssertions;
using InvoiceSample.WebApi.Dtos;
using RestSharp;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;

namespace InvoiceSample.WebApi.Tests;

public class InvoiceApiTests : IClassFixture<TestWebApplicationFactory>
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        ReferenceHandler = ReferenceHandler.Preserve
    };

    private readonly RestClient _client;

    public InvoiceApiTests(TestWebApplicationFactory factory)
    {
        _client = new RestClient(factory.CreateClient());
    }

    [Fact]
    public async Task AddSalesOrder_ThenGetInvoice_ReturnsInvoice()
    {
        var request = new SalesOrderRequested
        {
            AutoInvoice = true,
            Number = "SO1",
            CustomerId = Guid.NewGuid(),
            Lines =
            {
                new SalesOrderLine
                {
                    IsService = false,
                    Ordinal = 1,
                    NetValue = 100m,
                    VatValue = 23m,
                    GrossValue = 123m,
                    ProductId = Guid.NewGuid(),
                    Quantity = 1,
                    VatRate = InvoiceSample.Domain.VatRate.TwentyThree
                }
            }
        };

        var postRequest = new RestRequest("/SalesOrder").AddJsonBody(request);
        var postResponse = await _client.ExecutePostAsync(postRequest);
        postResponse.IsSuccessful.Should().BeTrue();
        var createdInvoice = Deserialize(postResponse);

        var invoiceNumber = createdInvoice.Number!;

        var getRequest = new RestRequest("/Invoice/{number}").AddUrlSegment("number", invoiceNumber);
        var getResponse = await _client.ExecuteGetAsync(getRequest);
        getResponse.IsSuccessful.Should().BeTrue();
        var retrievedInvoice = Deserialize(getResponse);
        retrievedInvoice.Number.Should().Be(invoiceNumber);
    }

    [Fact]
    public async Task AddWarehouseRelease_UpdatesInvoice()
    {
        var soRequest = new SalesOrderRequested
        {
            AutoInvoice = true,
            Number = "SO2",
            CustomerId = Guid.NewGuid(),
            Lines =
            {
                new SalesOrderLine
                {
                    IsService = false,
                    Ordinal = 1,
                    NetValue = 100m,
                    VatValue = 23m,
                    GrossValue = 123m,
                    ProductId = Guid.NewGuid(),
                    Quantity = 1,
                    VatRate = InvoiceSample.Domain.VatRate.TwentyThree
                }
            }
        };

        var soPost = new RestRequest("/SalesOrder").AddJsonBody(soRequest);
        var soResponse = await _client.ExecutePostAsync(soPost);
        soResponse.IsSuccessful.Should().BeTrue();
        var invoiceNumber = Deserialize(soResponse).Number!;

        var wrRequest = new WarehouseReleaseRequested
        {
            SalesOrderNumber = soRequest.Number,
            Number = "WR1",
            CustomerId = soRequest.CustomerId,
            Lines =
            {
                new WarehouseReleaseRequested.WarehouseReleaseLine
                {
                    SalesOrderLineOrdinal = 1,
                    Ordinal = 1,
                    NetValue = 100m,
                    VatValue = 23m,
                    GrossValue = 123m,
                    ProductId = Guid.NewGuid(),
                    Quantity = 1,
                    VatRate = InvoiceSample.Domain.VatRate.TwentyThree
                }
            }
        };

        var wrPost = new RestRequest("/WarehouseMovement/warehouseRelease").AddJsonBody(wrRequest);
        var wrResponse = await _client.ExecutePostAsync(wrPost);
        wrResponse.IsSuccessful.Should().BeTrue();

        var getRequest = new RestRequest("/Invoice/{number}").AddUrlSegment("number", invoiceNumber);
        var getResponse = await _client.ExecuteGetAsync(getRequest);
        getResponse.IsSuccessful.Should().BeTrue();
        Deserialize(getResponse).GrossValue.Should().BeGreaterThan(0m);
    }

    private static InvoiceResponse Deserialize(RestResponse response)
    {
        response.Content.Should().NotBeNullOrWhiteSpace();
        return JsonSerializer.Deserialize<InvoiceResponse>(response.Content!, SerializerOptions)!;
    }
}


public class InvoiceResponse
{
    public string? Number { get; set; }
    public decimal GrossValue { get; set; }
}
