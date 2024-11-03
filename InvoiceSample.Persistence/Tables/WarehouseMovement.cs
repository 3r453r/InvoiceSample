using AutoMapper;
using InvoiceSample.DataDrivenEntity;
using InvoiceSample.Domain;
using InvoiceSample.Domain.WarehouseReleaseAggregate;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Persistence.Tables
{
    public class WarehouseMovement : Entity<WarehouseMovement, string, IWarehouseReleaseData>, IWarehouseReleaseData
    {
        public WarehouseMovement()
        {
            RegisterExternalChildCollection<WarehouseMovementLine, int, IWarehouseReleaseLine, IMapper>(
                Lines
                , d => d.Lines
                , (_, _) => new WarehouseMovementLine()
                , (_) => _mapper!
                );
        }

        public SalesOrder? SalesOrder { get; set; }
        public string SalesOrderNumber => SalesOrder?.Number ?? "";

        public List<WarehouseMovementLine> Lines { get; set; } = [];
        IEnumerable<IWarehouseReleaseLine> IWarehouseReleaseData.Lines => Lines;
        IEnumerable<IDocumentLine> IDocument.Lines => Lines;

        [MaxLength(50)]
        public string Number { get; set; } = "";

        public Guid CustomerId { get; set; }

        public WarehouseMovementType Type { get; set; }


        public override IWarehouseReleaseData GetEntityData() => this;
        object IEntityData.GetKey() => Number;

        public override object GetKey() => Number;

        string IEntityData<string>.GetKey() => Number;
    }

    public enum WarehouseMovementType : byte
    {
        WarehouseRelease,
        WarehouseReturn
    }
}
