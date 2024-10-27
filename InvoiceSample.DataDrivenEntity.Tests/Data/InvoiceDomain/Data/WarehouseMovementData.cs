﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity.Tests.Data.InvoiceDomain.Data
{
    public class WarehouseMovementData : IWarehouseMovementData, IEquatable<IWarehouseMovementData>
    {
        public WarehouseMovementType WarehouseMovementType { get; set; }

        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public DateTime Created { get; set; }
        public int CreatedBy { get; set; }

        public bool Equals(IWarehouseMovementData? other) => other is not null && Id == other.Id && Name == other.Name
                && Created == other.Created && CreatedBy == other.CreatedBy;

        public Guid GetKey() => Id;

        object IEntityData.GetKey() => Id;
    }
}
