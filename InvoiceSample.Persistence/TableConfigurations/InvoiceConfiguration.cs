using InvoiceSample.Persistence.Tables;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Persistence.TableConfigurations
{
    public static class InvoiceConfiguration
    {
        public static void ConfigureInvoice(this ModelBuilder modelBuilder) 
        {
            modelBuilder.Entity<Invoice>()
                .HasIndex(i => i.Number);
        }
    }
}
