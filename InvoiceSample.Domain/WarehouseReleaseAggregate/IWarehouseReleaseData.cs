using InvoiceSample.DataDrivenEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.Domain.WarehouseReleaseAggregate
{
    public interface IWarehouseReleaseData : IDocument, IEntityData<string>
    {
        string SalesOrderNumber { get; }
        new IEnumerable<IWarehouseReleaseLine> Lines { get; }
    }
}
