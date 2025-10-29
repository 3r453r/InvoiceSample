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
        postResponse.IsSuccessful.Should().BeTrue(
            $"Status: {(int)postResponse.StatusCode} {postResponse.StatusCode}; Error: {postResponse.ErrorMessage}; Content: {postResponse.Content}");
        var createdInvoice = Deserialize(postResponse);

        var invoiceNumber = createdInvoice.Number!;

        var getRequest = new RestRequest($"/Invoice/{Uri.EscapeDataString(invoiceNumber)}");
        var getResponse = await _client.ExecuteGetAsync(getRequest);
        getResponse.IsSuccessful.Should().BeTrue(
            $"Status: {(int)getResponse.StatusCode} {getResponse.StatusCode}; Error: {getResponse.ErrorMessage}; Content: {getResponse.Content}");
        var retrievedInvoice = Deserialize(getResponse);
        retrievedInvoice.Number.Should().Be(invoiceNumber);
    }

    [Fact]
    public async Task AddSalesOrderTwice_UpdatesInvoice()
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
        soResponse.IsSuccessful.Should().BeTrue(
            $"Status: {(int)soResponse.StatusCode} {soResponse.StatusCode}; Error: {soResponse.ErrorMessage}; Content: {soResponse.Content}");
        var invoiceNumber = Deserialize(soResponse).Number!;

        var updatedRequest = new SalesOrderRequested
        {
            AutoInvoice = true,
            Number = soRequest.Number,
            CustomerId = soRequest.CustomerId,
            Lines =
            {
                new SalesOrderLine
                {
                    Ordinal = 1,
                    IsService = false,
                    NetValue = 200m,
                    VatValue = 46m,
                    GrossValue = 246m,
                    ProductId = Guid.NewGuid(),
                    Quantity = 2,
                    VatRate = InvoiceSample.Domain.VatRate.TwentyThree
                }
            }
        };

        var updatePost = new RestRequest("/SalesOrder").AddJsonBody(updatedRequest);
        var updateResponse = await _client.ExecutePostAsync(updatePost);
        updateResponse.IsSuccessful.Should().BeTrue(
            $"Status: {(int)updateResponse.StatusCode} {updateResponse.StatusCode}; Error: {updateResponse.ErrorMessage}; Content: {updateResponse.Content}");
        var updatedInvoice = Deserialize(updateResponse);
        updatedInvoice.Number.Should().Be(invoiceNumber);

        var getRequest = new RestRequest($"/Invoice/{Uri.EscapeDataString(invoiceNumber)}");
        var getResponse = await _client.ExecuteGetAsync(getRequest);
        getResponse.IsSuccessful.Should().BeTrue(
            $"Status: {(int)getResponse.StatusCode} {getResponse.StatusCode}; Error: {getResponse.ErrorMessage}; Content: {getResponse.Content}");
        Deserialize(getResponse).Number.Should().Be(invoiceNumber);
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
