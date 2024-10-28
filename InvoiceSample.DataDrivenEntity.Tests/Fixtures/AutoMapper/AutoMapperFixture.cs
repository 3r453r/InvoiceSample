using AutoMapper;
using InvoiceSample.DataDrivenEntity.Extensions;
using System.Reflection;

namespace InvoiceSample.DataDrivenEntity.Tests.Fixtures.AutoMapper
{
    public class AutoMapperFixture
    {
        public IMapper Mapper { get; private set; }

        public AutoMapperFixture()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.GloballyIgnoreProperties(cfg =>
                {
                    cfg.Ignore<IDataDrivenEntityBase>()
                    .IgnoreCollections<IDataDrivenEntityBase>();
                });

                cfg.AddMaps(Assembly.GetExecutingAssembly());
            });

            Mapper = config.CreateMapper();
        }
    }
}
