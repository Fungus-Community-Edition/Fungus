using System;
using System.Collections.Generic;

namespace AtMycelia
{
    public interface ITagFilterable
    {
        IList<object> Filters { get; set; }
        bool PassesFilter(object tag, StringComparison strCompare = StringComparison.Ordinal);
    }

    public interface ITagFilterable<T> : ITagFilterable
    {
        new IList<T> Filters { get; set; }
        bool PassesFilter(T tag, StringComparison strCompare = StringComparison.Ordinal);
    }
}