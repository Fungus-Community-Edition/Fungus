using System.Collections.Generic;

namespace Amanita.VScripting.EditorUtils
{
    public interface IConnectionGatherer
    {
        IList<ConnectionInfo> GatherConnections(DrawBlockContext drawCtx);
    }
}