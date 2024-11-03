using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity
{
    public interface IDataDrivenEntityBase
    {
        object GetEntityData();
        bool IsInitialized { get; }
        object GetKey();
        bool IsNew { get; set; }
        IEnumerable<IDataDrivenEntityBase> GetAllEntities();
    }
}
