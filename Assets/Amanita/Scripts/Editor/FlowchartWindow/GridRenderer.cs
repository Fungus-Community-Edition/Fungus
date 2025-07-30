using UnityEngine;
using Amanita.EditorUtils;

namespace Amanita.VScripting.EditorUtils
{
    public class GridRenderer
    {
        public GridRenderer(ILineDrawer drawer)
        {
            _drawer = drawer;
        }

        protected readonly ILineDrawer _drawer;

        public void Draw(FlowchartContext ctx, DrawGridContext gridCtx)
        {
            var fc = ctx.Flowchart;
            var pos = ctx.Position;
            var spacing = gridCtx.GridLineSpacingSize;
            var color = gridCtx.GridLineColor;

            float width = pos.width / fc.Zoom;
            float height = pos.height / fc.Zoom;

            var xPositions = GridUtils.GetVerticalLinePositions(fc.ScrollPos.x, width, spacing);
            var yPositions = GridUtils.GetHorizontalLinePositions(fc.ScrollPos.y, height, spacing);

            var prevColor = _drawer.Color;
            _drawer.Color = color;

            foreach (var x in xPositions)
                _drawer.DrawLine(new Vector2(x, 0), new Vector2(x, height));

            foreach (var y in yPositions)
                _drawer.DrawLine(new Vector2(0, y), new Vector2(width, y));

            _drawer.Color = prevColor;
        }
    }
    public class DrawGridContext
    {
        public virtual float GridLineSpacingSize { get; set; }
        public virtual Color GridLineColor { get; set; }
    }
}