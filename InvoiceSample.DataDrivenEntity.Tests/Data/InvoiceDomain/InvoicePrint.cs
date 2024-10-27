using InvoiceSample.DataDrivenEntity.Implementations.Basic;
using InvoiceSample.DataDrivenEntity.Tests.Data.TestEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity.Tests.Data.InvoiceDomain
{
    public interface IInvoicePrintData : IBaseEntityData
    { }
    public class InvoicePrint : ChildlessSelfDataDataDrivenEntity<InvoicePrint, Guid, IInvoicePrintData>, IInvoicePrintData
    {
        private bool _initialized;

        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public DateTime Created { get; set; }
        public int CreatedBy { get; set; }
        protected override bool SelfInitialzed => _initialized;
        object IEntityData.GetKey() => Id;

        protected override void SelfInitialize(IInvoicePrintData entityData)
        {
            Id = entityData.Id;
            Name = entityData.Name;
            Created = entityData.Created;
            CreatedBy = entityData.CreatedBy;
            _initialized = true;
        }
    }
}
