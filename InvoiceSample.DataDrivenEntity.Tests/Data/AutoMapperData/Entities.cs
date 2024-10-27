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
    public interface IParentData : IBaseEntityData, IAggregateEntityData
    {
        IBaseEntityData? DataDrivenChild { get; }
        IEnumerable<IBaseEntityData> DataDrivenChildren { get; }

        IBaseEntityData? Child { get; }
        IEnumerable<IBaseEntityData> Children { get; }
    }

    public class Parent : ReflectiveSelfDataDataDrivenEntity<Parent, Guid, IParentData>, IParentData
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

        public IEnumerable<(IEntityData? Entity, string Selector)> ChildrenData => [];

        public IEnumerable<(IEnumerable<IEntityData> Collection, string Selector)> ChildrenCollectionsData => [];

        protected override bool SelfInitialzed => _selfInitialized;

        IBaseEntityData? IParentData.Child => Child;

        IEnumerable<IBaseEntityData> IParentData.Children => Children;

        IBaseEntityData? IParentData.DataDrivenChild => DataDrivenChild;

        IEnumerable<IBaseEntityData> IParentData.DataDrivenChildren => DataDrivenChildren;

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
                .ForMember(d => d.ChildrenData, x => x.Ignore())
                .ForMember(d => d.ChildrenCollectionsData, x => x.Ignore())
                ;
            CreateMap<IBaseEntityData, Child>();
            CreateMap<IBaseEntityData, DataDrivenChild>();
        }
    }

    public class DataDrivenChild : ChildlessSelfDataDataDrivenEntity<DataDrivenChild, Guid, IBaseEntityData>, IBaseEntityData
    {
        private bool _selfInitialized;

        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public DateTime Created { get; set; }
        public int CreatedBy { get; set; }
        object IEntityData.GetKey() => Id;

        protected override bool SelfInitialzed => _selfInitialized;

        protected override void SelfInitialize(IBaseEntityData entityData)
        {
            Id = entityData.Id;
            Name = entityData.Name;
            Created = entityData.Created;
            CreatedBy = entityData.CreatedBy;
            _selfInitialized = true;
        }

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
        public IBaseEntityData DataDrivenChild => new BaseEntityData { Id = Guid.NewGuid(), Name = "a", CreatedBy = 1, Created = DateTime.Now};

        public IEnumerable<IBaseEntityData> DataDrivenChildren => [new BaseEntityData { Id = Guid.NewGuid(), Name = "a", CreatedBy = 1, Created = DateTime.Now }];

        public IBaseEntityData Child => new BaseEntityData { Id = Guid.NewGuid(), Name = "a", CreatedBy = 1, Created = DateTime.Now };

        public IEnumerable<IBaseEntityData> Children => [new BaseEntityData { Id = Guid.NewGuid(), Name = "a", CreatedBy = 1, Created = DateTime.Now }];

        object IEntityData.GetKey() => Id;
        public Guid GetKey() => Id;

        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public DateTime Created { get; set; }
        public int CreatedBy { get; set; }

        public IEnumerable<(IEntityData? Entity, string Selector)> ChildrenData => [];

        public IEnumerable<(IEnumerable<IEntityData> Collection, string Selector)> ChildrenCollectionsData => [];
    }

    public class BaseEntityData : IBaseEntityData
    {
        object IEntityData.GetKey() => Id;
        public Guid GetKey() => Id;

        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public DateTime Created { get; set; }
        public int CreatedBy { get; set; }
    }
}
