using RestSharp;
using FluentAssertions;
using InvoiceSample.WebApi.Dtos;
using Xunit;

namespace InvoiceSample.WebApi.Tests;

public class InvoiceApiTests : IClassFixture<TestWebApplicationFactory>
{
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
        var postResponse = await _client.ExecutePostAsync<InvoiceResponse>(postRequest);
        postResponse.IsSuccessful.Should().BeTrue();
        var invoiceNumber = postResponse.Data!.Number;

        var getRequest = new RestRequest($"/Invoice/{invoiceNumber}");
        var getResponse = await _client.ExecuteGetAsync<InvoiceResponse>(getRequest);
        getResponse.IsSuccessful.Should().BeTrue();
        getResponse.Data!.Number.Should().Be(invoiceNumber);
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
        var soResponse = await _client.ExecutePostAsync<InvoiceResponse>(soPost);
        soResponse.IsSuccessful.Should().BeTrue();
        var invoiceNumber = soResponse.Data!.Number;

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
        var wrResponse = await _client.ExecutePostAsync<InvoiceResponse>(wrPost);
        wrResponse.IsSuccessful.Should().BeTrue();

        var getRequest = new RestRequest($"/Invoice/{invoiceNumber}");
        var getResponse = await _client.ExecuteGetAsync<InvoiceResponse>(getRequest);
        getResponse.IsSuccessful.Should().BeTrue();
        getResponse.Data!.GrossValue.Should().BeGreaterThan(0m);
    }
}


public class InvoiceResponse
{
    public string? Number { get; set; }
    public decimal GrossValue { get; set; }
}
