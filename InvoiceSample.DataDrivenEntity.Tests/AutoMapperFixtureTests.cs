using AutoMapper;
using InvoiceSample.DataDrivenEntity.Tests.Data.AutoMapperData;
using InvoiceSample.DataDrivenEntity.Tests.Fixtures.AutoMapper;

namespace InvoiceSample.DataDrivenEntity.Tests
{
    [Collection("AutoMapper")]
    public class AutoMapperFixtureTests
    {
        private readonly IMapper _mapper;

        public AutoMapperFixtureTests(AutoMapperFixture autoMapper)
        {
            _mapper = autoMapper.Mapper;
        }
        [Fact]
        public void AutoMapper_Doesnt_Map_DataDrivenEntities() 
        { 
            var parent = new Parent(_mapper);
            var data = new ParentData();
            data.DataDrivenChildren = [new DataDrivenChild()];
            data.Children = [new Child()];
            data.Child = new Child();
            data.DataDrivenChild = new DataDrivenChild();

            parent.Initialize(data);

            Assert.Null(parent.DataDrivenChild);
            Assert.Empty(parent.DataDrivenChildren);
            Assert.NotNull(parent.Child);
            Assert.NotEmpty(parent.Children);
        }
    }
}
