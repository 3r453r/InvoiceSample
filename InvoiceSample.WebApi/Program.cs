using InvoiceSample.Application.EventBus;
using InvoiceSample.Application.Persistence;
using InvoiceSample.Application.Services.Invoice;
using InvoiceSample.DataDrivenEntity;
using InvoiceSample.DataDrivenEntity.Extensions;
using InvoiceSample.DataDrivenEntity.Implementations.Basic;
using InvoiceSample.Persistence;
using InvoiceSample.Persistence.ApplicationImplementtion;
using InvoiceSample.Persistence.Extensions;
using InvoiceSample.Persistence.Tables;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<InvoiceSampleDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
});

builder.Services.AddAutoMapper(cfg => {
    cfg.GloballyIgnoreProperties(cfg =>
    {
        cfg.Ignore<IDataDrivenEntityBase>()
        .IgnoreCollections<IDataDrivenEntityBase>()
        .Ignore<IEntityData>()
        .IgnoreCollections<IEntityData>();
    });
    },
    typeof(InvoiceSample.Domain.MappingProfiles.InvoiceMappingProfile).Assembly
    , typeof(InvoiceSample.Persistence.MappingProfiles.InvoiceMappingProfile).Assembly);

builder.Services.AddScoped<IInvoiceSampleUnitOfWork, InvoiceSampleUnitOfWork>();
builder.Services.AddSingleton<IEventBus, MockEventBus>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseMigrations();

app.Run();
