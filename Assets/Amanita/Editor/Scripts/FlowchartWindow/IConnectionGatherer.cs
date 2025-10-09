using System;
using System.Collections.Generic;

namespace Amanita.VScripting.EditorUtils
{
    public interface IConnectionGatherer : IDisposable
    {
        IList<ConnectionInfo> GatherConnections(DrawBlockContext drawCtx);
    }
}