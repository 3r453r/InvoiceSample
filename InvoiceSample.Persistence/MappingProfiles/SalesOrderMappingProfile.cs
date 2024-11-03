using AutoMapper;
using InvoiceSample.Domain.SalesOrderAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Persistence.MappingProfiles
{
    public class SalesOrderMappingProfile : Profile
    {
        public SalesOrderMappingProfile()
        {
            CreateMap<ISalesOrderData, Tables.SalesOrder>();
            CreateMap<ISalesOrderLine, Tables.SalesOrderLine>();
        }
    }
}
