using AutoMapper;
using InvoiceSample.Domain.InvoiceAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Domain.MappingProfiles
{
    public class InvoiceMappingProfile : Profile
    {
        public InvoiceMappingProfile()
        {
            CreateMap<IInvoiceData, Invoice>();
            CreateMap<IInvoiceLine, InvoiceLine>();
            CreateMap<IVatSum, VatSum>();
        }
    }
}
