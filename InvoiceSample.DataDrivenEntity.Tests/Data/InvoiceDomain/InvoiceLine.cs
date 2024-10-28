using InvoiceSample.DataDrivenEntity.Implementations.Basic;
using InvoiceSample.DataDrivenEntity.Tests.Data.TestEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity.Tests.Data.InvoiceDomain
{
    public interface IInvoiceLineData : IBaseEntityData
    {
        IDictionaryValueData Status { get; }
    }
    public class InvoiceLine : DataDrivenEntity<InvoiceLine, Guid, IInvoiceLineData>, IInvoiceLineData
    {
        private bool _initialized;

        public InvoiceLine()
        {
            RegisterChild<DictionaryValue, Guid, IDictionaryValueData, IInvoiceLineData, Guid>(Status, id => id.Status, (_) => { throw new InvalidOperationException("Status cannot be null"); }, (s) => { Status = (DictionaryValue)s; }, () => new DictionaryValue());    
        }

        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public DateTime Created { get; set; }
        public int CreatedBy { get; set; }
        public DictionaryValue Status { get; set; } = new DictionaryValue();
        protected override bool SelfInitialzed => _initialized;
        IDictionaryValueData IInvoiceLineData.Status => Status;

        object IEntityData.GetKey() => Id;

        protected override void SelfInitialize(IInvoiceLineData entityData)
        {
            Id = entityData.Id;
            Name = entityData.Name;
            Created = entityData.Created;
            CreatedBy = entityData.CreatedBy;
            _initialized = true;
        }

        public override IInvoiceLineData GetEntityData() => this;

        public override Guid GetKey() => Id;
    }
}
