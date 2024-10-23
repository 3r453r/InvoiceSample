using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Persistence.Tables
{
    public abstract class Entity
    {
        public Guid Id { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;

        public abstract void UpdateCollections<TEntityData>(TEntityData entityData, DbContext dbContext);
    }
}
