using AutoMapper;
using InvoiceSample.DataDrivenEntity.Aggregates;
using InvoiceSample.DataDrivenEntity.Implementations.Basic;
using InvoiceSample.DataDrivenEntity.Tests.Data.TestEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvoiceSample.DataDrivenEntity.Tests.Data.AutoMapperData
{
    public interface IParentData : IBaseEntityData
    {
        IDataDrivenChildData? DataDrivenChild { get; }
        IEnumerable<IDataDrivenChildData> DataDrivenChildren { get; }

        IBaseEntityData? Child { get; }
        IEnumerable<IBaseEntityData> Children { get; }
    }

    public class Parent : DataDrivenEntity<Parent, Guid, IParentData>, IParentData
    {
        private readonly IMapper? _mapper;
        private bool _selfInitialized;

        public Parent() { }

        public Parent(IMapper mapper)
        {
            _mapper = mapper;
        }

        public DataDrivenChild? DataDrivenChild { get; set; }
        public Child? Child { get; set; }

        public List<DataDrivenChild> DataDrivenChildren { get; set; } = [];
        public List<Child> Children { get; set; } = [];

        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public DateTime Created { get; set; }
        public int CreatedBy { get; set; }

        protected override bool SelfInitialzed => _selfInitialized;

        IDataDrivenChildData? IParentData.DataDrivenChild => DataDrivenChild;

        IEnumerable<IDataDrivenChildData> IParentData.DataDrivenChildren => DataDrivenChildren;

        IBaseEntityData? IParentData.Child => Child;

        IEnumerable<IBaseEntityData> IParentData.Children => Children;

        public override IParentData GetEntityData() => this;

        public override Guid GetKey() => Id;

        protected override void SelfInitialize(IParentData entityData)
        {
            if (_mapper is not null)
            {
                _mapper.Map(entityData, this);
            }
        }

        object IEntityData.GetKey() => Id;
    }

    public class ParentMappingProfile : Profile
    {
        public ParentMappingProfile()
        {
            CreateMap<IParentData, Parent>()
                ;
            CreateMap<IBaseEntityData, Child>();
            CreateMap<IDataDrivenChildData, DataDrivenChild>();
        }
    }

    public interface IDataDrivenChildData : IBaseEntityData
    { }
    public class DataDrivenChild : DataDrivenEntity<DataDrivenChild, Guid, IDataDrivenChildData>, IDataDrivenChildData
    {
        private bool _selfInitialized;

        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public DateTime Created { get; set; }
        public int CreatedBy { get; set; }
        object IEntityData.GetKey() => Id;

        public override IDataDrivenChildData GetEntityData() => this;

        public override Guid GetKey() => Id;

        protected override void SelfInitialize(IDataDrivenChildData entityData)
        {
            Id = entityData.Id;
            Name = entityData.Name;
            Created = entityData.Created;
            CreatedBy = entityData.CreatedBy;
            _selfInitialized = true;
        }

        protected override bool SelfInitialzed => _selfInitialized;

    }

    public class Child : IBaseEntityData
    {
        object IEntityData.GetKey() => Id;
        public Guid GetKey() => Id;

        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public DateTime Created { get; set; }
        public int CreatedBy { get; set; }
    }

    public class ParentData : IParentData
    {
        object IEntityData.GetKey() => Id;
        public Guid GetKey() => Id;

        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public DateTime Created { get; set; }
        public int CreatedBy { get; set; }

        public Child? Child { get; set; }
        public DataDrivenChild? DataDrivenChild { get; set; }
        public List<DataDrivenChild> DataDrivenChildren { get; set; } = [];
        public List<Child> Children { get; set; } = [];

        IDataDrivenChildData? IParentData.DataDrivenChild => DataDrivenChild;

        IEnumerable<IDataDrivenChildData> IParentData.DataDrivenChildren => DataDrivenChildren;

        IBaseEntityData? IParentData.Child => Child;

        IEnumerable<IBaseEntityData> IParentData.Children => Children;
    }
}
