using InvoiceSample.Persistence.TableConfigurations;
using InvoiceSample.Persistence.Tables;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Persistence
{
    public class InvoiceSampleDbContext : DbContext
    {
        public InvoiceSampleDbContext(DbContextOptions<InvoiceSampleDbContext> options) : base(options)
        {
        }

        protected InvoiceSampleDbContext()
        {
        }

        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceLine> InvoiceLines { get; set; }
        public DbSet<InvoiceVatSum> InvoiceVatSums { get; set; }
        public DbSet<SalesOrder> SalesOrders { get; set; }
        public DbSet<SalesOrderLine> SalesOrderLines { get; set; }
        public DbSet<WarehouseMovement> WarehouseMovements { get; set; }
        public DbSet<WarehouseMovementLine> WarehouseMovementLines { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ConfigureInvoice();
        }
    }
}
