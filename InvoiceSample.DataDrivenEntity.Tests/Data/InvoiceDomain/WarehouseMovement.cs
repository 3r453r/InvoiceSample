using InvoiceSample.DataDrivenEntity.Implementations.Basic;
using InvoiceSample.DataDrivenEntity.Tests.Data.TestEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity.Tests.Data.InvoiceDomain
{
    public interface IWarehouseMovementData : IBaseEntityData
    {
        WarehouseMovementType WarehouseMovementType { get; }
    }

    public enum WarehouseMovementType
    {
        WarehouseRelease,
        WarehouseReturn
    }

    public class WarehouseMovement : DataDrivenEntity<WarehouseMovement, Guid, IWarehouseMovementData>, IWarehouseMovementData
    {
        private bool _initialized;

        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public DateTime Created { get; set; }
        public int CreatedBy { get; set; }
        protected override bool SelfInitialzed => _initialized;

        public WarehouseMovementType WarehouseMovementType { get; set; }

        object IEntityData.GetKey() => Id;

        protected override void SelfInitialize(IWarehouseMovementData entityData)
        {
            Id = entityData.Id;
            Name = entityData.Name;
            Created = entityData.Created;
            CreatedBy = entityData.CreatedBy;
            WarehouseMovementType = entityData.WarehouseMovementType;
            _initialized = true;
        }

        public override IWarehouseMovementData GetEntityData() => this;

        public override Guid GetKey() => Id;
    }
}
