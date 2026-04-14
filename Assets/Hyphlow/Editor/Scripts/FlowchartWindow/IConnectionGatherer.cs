using System;
using System.Collections.Generic;

namespace AtMycelia.Hyphlow.EditorUtils
{
    public interface IConnectionGatherer : IDisposable
    {
        IList<ConnectionInfo> GatherConnections(DrawBlockContext drawCtx);
    }
}