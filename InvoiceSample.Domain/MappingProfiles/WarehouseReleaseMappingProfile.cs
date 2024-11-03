using AutoMapper;
using InvoiceSample.Domain.WarehouseReleaseAggregate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Domain.MappingProfiles
{
    public class WarehouseReleaseMappingProfile : Profile
    {
        public WarehouseReleaseMappingProfile() 
        {
            CreateMap<IWarehouseReleaseData, WarehouseRelease>();
            CreateMap<IWarehouseReleaseLine, WarehouseReleaseLine>();
        }
    }
}
