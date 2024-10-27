using AutoMapper;
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
                cfg.ShouldMapProperty = p => !(
                    p.PropertyType.IsAssignableTo(typeof(IDataDrivenEntity))
                    || p.PropertyType.IsAssignableTo(typeof(IExternalDataDrivenEntity))
                    || p.PropertyType.IsAssignableTo(typeof(IEnumerable<IDataDrivenEntity>))
                    || p.PropertyType.IsAssignableTo(typeof(IEnumerable<IExternalDataDrivenEntity>))
                    );

                cfg.AddMaps(Assembly.GetExecutingAssembly());
            });

            Mapper = config.CreateMapper();
        }
    }
}
