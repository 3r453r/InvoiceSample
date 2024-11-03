using AutoMapper;
using InvoiceSample.Domain.WarehouseReleaseAggregate;
using InvoiceSample.Persistence.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Persistence.MappingProfiles
{
    public class WarehouseReleaseMappingProfile : Profile
    {
        public WarehouseReleaseMappingProfile()
        {
            CreateMap<IWarehouseReleaseData, WarehouseMovement>();
            CreateMap<IWarehouseReleaseLine, WarehouseMovementLine>();
        }
    }
}
