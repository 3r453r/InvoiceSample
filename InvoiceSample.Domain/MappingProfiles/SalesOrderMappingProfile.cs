using AutoMapper;
using InvoiceSample.Domain.SalesOrderAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Domain.MappingProfiles
{
    public class SalesOrderMappingProfile : Profile
    {
        public SalesOrderMappingProfile()
        {
            CreateMap<ISalesOrderData, SalesOrder>();
            CreateMap<ISalesOrderLine, SalesOrderLine>();
        }
    }
}
