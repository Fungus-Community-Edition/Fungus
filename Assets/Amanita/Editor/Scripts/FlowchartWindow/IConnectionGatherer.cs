using System;
using System.Collections.Generic;

namespace AtMycelia.Amanita.VScripting.EditorUtils
{
    public interface IConnectionGatherer : IDisposable
    {
        IList<ConnectionInfo> GatherConnections(DrawBlockContext drawCtx);
    }
}