using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using InvoiceSample.DataDrivenEntity;
using InvoiceSample.DataDrivenEntity.Implementations;

namespace InvoiceSample.Domain.WarehouseReleaseAggregate
{
    public class WarehouseRelease : ExternalDataDrivenEntity<string, IWarehouseReleaseData, IMapper>
        , IWarehouseReleaseData
    {
        private bool _initialized;
        private IMapper? _mapper;

        public WarehouseRelease()
        {
            RegisterExternalChildCollection<WarehouseReleaseLine, int, IWarehouseReleaseLine, WarehouseReleaseLineExternalData>(
                Lines
                , wd => wd.Lines
                , (_, _) => new WarehouseReleaseLine()
                , (_) => new WarehouseReleaseLineExternalData 
                {
                    Mapper = _mapper ?? throw new ArgumentNullException(nameof(Mapper)),
                    WarehouseRelease = this,
                });    
        }

        public WarehouseRelease(IWarehouseReleaseData wd, IMapper mapper) : this()
        {
            _mapper = mapper;
            Initialize(wd, mapper, true);
            _initialized = true;
        }

        public string Number { get; private set; } = "";

        public Guid CustomerId { get; private set; }

        public string SalesOrderNumber { get; private set; } = "";

        public List<WarehouseReleaseLine> Lines { get; init; } = [];

        protected override bool SelfInitialzed => _initialized;

        IEnumerable<IDocumentLine> IDocument.Lines => Lines;
        IEnumerable<IWarehouseReleaseLine> IWarehouseReleaseData.Lines => Lines;

        public override IWarehouseReleaseData GetEntityData() => this;

        public override string GetKey() => Number;
        object IEntityData.GetKey() => Number;

        protected override void SelfInitialize(IWarehouseReleaseData entityData, IMapper externalData)
        {
            externalData.Map(entityData, this);
            _mapper = externalData;
            _initialized = true;
        }
    }
}
