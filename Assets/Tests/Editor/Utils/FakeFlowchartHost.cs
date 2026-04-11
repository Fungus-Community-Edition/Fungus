using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
using AtMycelia.Hyphlow.EditorUtils.FcWindow;

namespace AtMycelia.Hyphlow.EditorUtils
{
    public class FakeFlowchartHost : IFlowchartHost, IDisposable
    {
        public virtual void Init()
        {
            Flowchart = new GameObject("fc").AddComponent<Flowchart>();
            EnsureWindowConfig();

            window = ScriptableObject.CreateInstance<TestFlowchartWindowUitk>();
            rootVisualElement = window.rootVisualElement;
            rootVisualElement.name = "FakeFlowchartHostRoot";
            rootVisualElement.style.flexGrow = 1f;
            rootVisualElement.style.width = Length.Percent(100f);
            rootVisualElement.style.height = Length.Percent(100f);

            UpdateContexts();
            SetWindowContext(window, FlowchartCtx);

            inputSignals = window.InputSignals;
            inputSignals.Initialize(window);

            blockDrawer = new FakeBlockDrawerUitk();
            graphicsRenderer = new FcwGraphicsRenderer(FlowchartCtx, DrawGridCtx, blockDrawer);
            viewportHandlers = new MainViewportManager(FlowchartCtx, FlowchartWindow.Config.MinZoom, FlowchartWindow.Config.MaxZoom);

            rootVisualElement.Add(graphicsRenderer);

            graphicsRenderer.Initialize(window);
            viewportHandlers.Initialize(window);
        }

        private void UpdateContexts()
        {
            FlowchartCtx.FcHost = this;
            FlowchartCtx.Flowchart = Flowchart;
            FlowchartCtx.Position = position;
            FlowchartCtx.GridObjectSnap = 10f;

            DrawGridCtx.GridLineSpacingSize = 120;
            DrawGridCtx.GridLineColor = GridLineColor;

            DrawBlockCtx.FlowchartCtx = FlowchartCtx;
            DrawBlockCtx.DefaultBlockHeight = 40;
            DrawBlockCtx.BlockMinWidth = 60;
            DrawBlockCtx.BlockMaxWidth = 240;
            DrawBlockCtx.ViewRect = CalcFlowchartWindowViewRect();
        }

        public Flowchart Flowchart { get; protected set; }
        public BlockClipboard Clipboard { get; set; } = new BlockClipboard(null);
        public bool HasClipboard => Clipboard.HasEntries;

        public Block CreateBlock(Flowchart fc, Vector2 pos)
        {
            var newBlock = fc.CreateBlock(pos);
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

        public void UpdateBlockCollection()
        {
            graphicsRenderer?.RefreshNow();
        }

        public void Repaint()
        {
            FlowchartCtx.ForceRepaintCount++;
        }

        public virtual void Dispose()
        {
            inputSignals?.Dispose();
            graphicsRenderer?.Dispose();
            viewportHandlers?.Dispose();

            if (window != null)
            {
                ScriptableObject.DestroyImmediate(window);
                window = null;
            }

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

        public virtual Vector2 GetBlockCenter(IReadOnlyCollection<Block> blocks)
        {
            return Vector2.zero;
        }

        public void OnGUI()
        {
            inputSignals?.OnGUI(Event.current);
            viewportHandlers?.OnGUI(Event.current);
        }

        public Rect CalcFlowchartWindowViewRect()
        {
            return new Rect(0f, 0f, position.width, position.height);
        }

        public void DoZoom(float delta, Vector2 center)
        {
            if (Flowchart == null)
            {
                return;
            }

            Flowchart.Zoom = Mathf.Max(0.01f, Flowchart.Zoom + delta);
            FlowchartWindowSignals.ZoomChanged(Flowchart.Zoom);
        }

        public void CenterFlowchart()
        {
            if (Flowchart == null)
            {
                return;
            }

            Flowchart.ScrollPos = Vector2.zero;
            FlowchartWindowSignals.WindowPanned();
        }

        public void SelectBlock(Block block)
        {
            if (block == null || Flowchart == null)
            {
                return;
            }

            Flowchart.AddToSelection(block);
        }

        public virtual DrawGridContext DrawGridCtx { get; protected set; } = new DrawGridContext();
        public virtual DrawBlockContext DrawBlockCtx { get; protected set; } = new DrawBlockContext();

        public Color GridLineColor { get; set; }

        public FlowchartContext FlowchartCtx { get; protected set; } = new FlowchartContext();

        public IReadOnlyCollection<Block> Blocks
        {
            get => Flowchart != null ? Flowchart.Blocks : Array.Empty<Block>();
        }

        public Rect Position => position;
        private Rect position = new Rect(0f, 0f, 200f, 200f);

        public void SetPosition(Rect newPosition)
        {
            position = newPosition;
            UpdateContexts();
        }

        public VisualElement RootVisualElement => rootVisualElement;
        private VisualElement rootVisualElement;

        public FcwGraphicsRenderer GraphicsRenderer => graphicsRenderer;
        public MainViewportManager ViewportHandlers => viewportHandlers;
        public InputSignalModule InputSignals => inputSignals;

        private FlowchartWindow window;
        private FcwGraphicsRenderer graphicsRenderer;
        private MainViewportManager viewportHandlers;
        private InputSignalModule inputSignals;
        private IBlockDrawerUitk blockDrawer;

        private sealed class FakeBlockDrawerUitk : IBlockDrawerUitk
        {
            public BlockButton CreateButton(Block block)
            {
                var button = new BlockButton(new BlockGraphicsGenerator());
                button.Initialize(block, FlowchartWindow.Config?.BlockUxml,
                    FlowchartWindow.Config?.BlockStyleSheet, FlowchartWindow.Config?.SelectedBlockStyleSheet);
                return button;
            }

            public void UpdateButton(BlockButton button, Block block, float zoom)
            {
                button?.UpdateVisuals(block, zoom);
            }
        }

        private sealed class TestFlowchartWindowUitk : FlowchartWindow
        {
            protected override void OnEnable()
            {
            }

            protected override void OnDestroy()
            {
            }
        }

        private static void EnsureWindowConfig()
        {
            if (FlowchartWindow.Config != null)
            {
                return;
            }

            var config = ScriptableObject.CreateInstance<FlowchartWindowConfig>();
            SetStaticConfig(config);
        }

        private static void SetStaticConfig(FlowchartWindowConfig config)
        {
            PropertyInfo property = typeof(FlowchartWindow).GetProperty("Config",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            if (property != null)
            {
                property.SetValue(null, config);
                return;
            }

            FieldInfo field = typeof(FlowchartWindow).GetField("<Config>k__BackingField",
                BindingFlags.Static | BindingFlags.NonPublic);

            field?.SetValue(null, config);
        }

        private static void SetWindowContext(FlowchartWindow targetWindow, FlowchartContext context)
        {
            FieldInfo field = typeof(FlowchartWindow).GetField("_fcContext",
                BindingFlags.Instance | BindingFlags.NonPublic);

            field?.SetValue(targetWindow, context);
        }
    }
}