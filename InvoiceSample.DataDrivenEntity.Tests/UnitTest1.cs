using InvoiceSample.DataDrivenEntity.Tests.Data.InvoiceDomain;

namespace InvoiceSample.DataDrivenEntity.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void DictionaryValue_Is_IEntityData()
        {
            var dv = new DictionaryValue
            {
                Id = Guid.NewGuid(),
            };

            var data = (IEntityData)dv;
        }
    }
}