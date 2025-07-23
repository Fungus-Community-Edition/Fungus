using System.Collections.Generic;

namespace Amanita.EditorUtils
{
    public interface IConnectionGatherer
    {
        IList<ConnectionInfo> GatherConnections(DrawBlockContext drawCtx);
    }
}