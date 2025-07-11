namespace Amanita.VScripting.EditorUtils
{
    public class ConnectionRenderer
    {
        public ConnectionRenderer(IConnectionDrawer connectionDrawer)
        {
            _drawer = connectionDrawer;
        }
        protected readonly IConnectionDrawer _drawer;

        public virtual void Render(DrawBlockContext drawCtx, FlowchartContext fcContext)
        {
            _drawer.Draw(drawCtx, fcContext);

        }

    }

}