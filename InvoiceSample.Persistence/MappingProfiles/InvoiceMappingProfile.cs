using AutoMapper;
using InvoiceSample.Domain.InvoiceAggregate;
using InvoiceSample.Persistence.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Persistence.MappingProfiles
{
    public class InvoiceMappingProfile : Profile
    {
        public InvoiceMappingProfile()
        {
            CreateMap<IInvoiceData, Tables.Invoice>();
            CreateMap<IInvoiceLine, Tables.InvoiceLine>();
            CreateMap<IVatSum, InvoiceVatSum>();
        }
    }
}
