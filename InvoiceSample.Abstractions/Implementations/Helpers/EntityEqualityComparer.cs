using System;
using System.Collections.Generic;

namespace InvoiceSample.DataDrivenEntity.Implementations.Helpers
{
    /// <summary>
    /// Custom equality comparer for data-driven entities that compares based on entity type and key
    /// </summary>
    public class EntityEqualityComparer : IEqualityComparer<IDataDrivenEntityBase>
    {
        public bool Equals(IDataDrivenEntityBase x, IDataDrivenEntityBase y)
        {
            if (x == null && y == null)
                return true;
            if (x == null || y == null)
                return false;

            // Compare based on type and key equality
            return x.GetType() == y.GetType() && x.GetKey().Equals(y.GetKey());
        }

        public int GetHashCode(IDataDrivenEntityBase obj)
        {
            if (obj == null)
                return 0;

            // Generate hash code based on the combination of type and key
            return obj.GetType().GetHashCode() ^ obj.GetKey().GetHashCode();
        }
    }
}