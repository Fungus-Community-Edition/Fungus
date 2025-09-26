using System;

namespace Amanita.VScripting.EditorUtils
{
    public class ConnectionRenderer : IDisposable
    {
        public virtual void Dispose()
        {
            _drawer = null;
        }

        public ConnectionRenderer(IConnectionDrawer connectionDrawer)
        {
            _drawer = connectionDrawer;
        }

        protected IConnectionDrawer _drawer;

        public virtual void Render(DrawBlockContext drawCtx, FlowchartContext fcContext)
        {
            _drawer.Draw(drawCtx, fcContext);

        }

    }

}