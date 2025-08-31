using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Amanita.VScripting;
using Amanita.VScripting.EditorUtils;
using UnityEngine.UIElements;

namespace Amanita.EditorUtils
{
    public class FakeFlowchartHost : IFlowchartHost, IDisposable
    {
        public virtual void Init()
        {
            Flowchart = new GameObject("fc").AddComponent<Flowchart>();
            components.Add(new FcWindowCanvas());
            components.Add(new FcWindowEditing());

            UpdateContexts();
            void UpdateContexts()
            {
                FlowchartCtx.FcHost = this;
                FlowchartCtx.Flowchart = Flowchart;
                FlowchartCtx.Position = Position;

                DrawGridCtx.GridLineSpacingSize = 120;
                DrawGridCtx.GridLineColor = GridLineColor;

                DrawBlockCtx.FlowchartCtx = FlowchartCtx;
                DrawBlockCtx.DefaultBlockHeight = 40;
                DrawBlockCtx.BlockMinWidth = 60;
                DrawBlockCtx.BlockMaxWidth = 240;
                DrawBlockCtx.ViewRect = CalcFlowchartWindowViewRect();
            }

            foreach (var elem in components)
            {
                elem.Initialize(this);
            }
        }

        public Flowchart Flowchart { get; protected set; }
        public BlockClipboard Clipboard { get; set; } = new BlockClipboard(null);
        public bool HasClipboard => Clipboard.HasEntries;

        public Block CreateBlock(Flowchart fc, Vector2 pos)
        {
            var newBlock = fc.CreateBlock(pos);
            // give it a visible area for hit‐testing
            newBlock._NodeRect = new Rect(pos, defaultNodeSize);
            created.Add(newBlock);
            fc.AddToSelection(newBlock);
            return newBlock;
        }

        protected readonly static Vector2 defaultNodeSize = new Vector2(20, 20);
        public List<Block> Created { get { return new List<Block>(created); } }
        protected IList<Block> created = new List<Block>();

        public void DeselectAll() => Flowchart.ClearSelectedBlocks();


        public IList<Block> QueuedForDeletion { get { return new List<Block>(queuedForDeletion); } }
        protected IList<Block> queuedForDeletion = new List<Block>();

        public void DeleteScheduledBlocks()
        {
            foreach (var block in QueuedForDeletion)
            {
                GameObject.DestroyImmediate(block.gameObject);
            }
            queuedForDeletion.Clear();
        }

        public void UpdateBlockCollection() { /* no-op for tests */ }
        public void Repaint() { /* no-op for tests */ }

        public virtual void Dispose()
        {
            Clipboard = null;
            queuedForDeletion.Clear();
            created.Clear();

            if (Flowchart != null)
            {
                GameObject.DestroyImmediate(Flowchart.gameObject);
            }
        }

        public T GetComponent<T>() where T : IFcWindowComponent
        {
            return components.OfType<T>().FirstOrDefault();
        }

        protected IList<IFcWindowComponent> components = new List<IFcWindowComponent>();

        public virtual Vector2 GetBlockCenter(IList<Block> blocks)
        {
            return Vector2.zero;
        }

        public void OnGUI()
        {
            
        }

        public Rect CalcFlowchartWindowViewRect()
        {
            return Rect.zero;
        }

        public void DoZoom(float delta, Vector2 center)
        {
            
        }

        public void CenterFlowchart()
        {
            
        }

        public void SelectBlock(Block block)
        {
            block.IsSelected = true;
        }

        public virtual DrawGridContext DrawGridCtx { get; protected set; } = new DrawGridContext();
        public virtual DrawBlockContext DrawBlockCtx { get; protected set; } = new DrawBlockContext();

        public Color GridLineColor { get; set; }

        public FlowchartContext FlowchartCtx { get; protected set; } = new FlowchartContext();

        public IList<Block> Blocks { get { return Flowchart.GetComponents<Block>(); } }

        public Rect Position => Rect.zero;

        public VisualElement RootVisualElement => throw new NotImplementedException();
    }
}