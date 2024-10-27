using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity.Tests.Data.TestEntities
{
    public interface IBaseEntityData : IEntityData<Guid>
    {
        Guid Id { get; set; }
        string Name { get; set; }
        DateTime Created { get; set; }
        int CreatedBy { get; set; }
    }
}
