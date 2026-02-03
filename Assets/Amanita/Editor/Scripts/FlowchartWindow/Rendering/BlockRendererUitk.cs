using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using MeasureMode = UnityEngine.UIElements.VisualElement.MeasureMode;

namespace Amanita.VScripting.EditorUtils
{
    public interface IBlockDrawerUitk
    {
        Button CreateButton(Block block);
        void UpdateButton(Button button, Block block, float zoom);
    }

    /// <summary>
    /// Renders Flowchart blocks as UITK buttons that size themselves to their contents.
    /// </summary>
    public sealed class BlockRendererUitk : VisualElement, IFlowchartWindowModule, IDisposable,
        IFlowchartChangeResponder, IWindowPanResponder, IScrollWheelMoveResponder,
        IBlockSelectionResponder, IPreBlockDeletionResponder
    {
        private readonly FlowchartContext flowchartContext;
        private readonly IBlockDrawerUitk drawer;
        private readonly Dictionary<Block, BlockBinding> blockBindings = new();
        private FlowchartWindowUitk owner;
        private bool isDisposed;

        /// <summary>
        /// Binds a block to its visual representation and event handlers.
        /// </summary>
        private sealed class BlockBinding
        {
            public Button Button;
            public Action ClickHandler;
        }

        public BlockRendererUitk(FlowchartContext context, IBlockDrawerUitk blockDrawer)
        {
            flowchartContext = context ?? throw new ArgumentNullException(nameof(context));
            drawer = blockDrawer ?? throw new ArgumentNullException(nameof(blockDrawer));

            pickingMode = PickingMode.Ignore;
            style.position = Position.Absolute;
            style.flexGrow = 1f;
        }

        public void Initialize(FlowchartWindowUitk window)
        {
            owner = window;
            RefreshBlocks();
        }

        public void RefreshBlocks()
        {
            if (isDisposed)
            {
                return;
            }

            Flowchart flowchart = flowchartContext.Flowchart;
            if (flowchart == null)
            {
                ClearAll();
                return;
            }

            IReadOnlyCollection<Block> present = flowchartContext.Document.AllBlocks;
            RemoveMissing(present);

            foreach (var block in present)
            {
                EnsureBlockVisual(block);
            }

            UpdateBlockLayouts();
        }

        private void RemoveMissing(IReadOnlyCollection<Block> currentBlocks)
        {
            using ListPool<Block>.DisposableList pooledKeysHandle = ListPool<Block>.Get(out List<Block> pooledKeys);
            pooledKeys.AddRange(blockBindings.Keys);
            for (int i = 0; i < pooledKeys.Count; i++)
            {
                Block tracked = pooledKeys[i];
                if (!ContainsBlock(currentBlocks, tracked))
                {
                    RemoveBlock(tracked);
                }
            }
        }

        private static bool ContainsBlock(IReadOnlyCollection<Block> blocks, Block target)
        {
            if (blocks == null)
            {
                return false;
            }

            if (blocks is ICollection<Block> collection)
            {
                return collection.Contains(target);
            }

            foreach (var block in blocks)
            {
                if (ReferenceEquals(block, target))
                {
                    return true;
                }
            }

            return false;
        }

        private void RemoveBlock(Block block)
        {
            if (!blockBindings.TryGetValue(block, out BlockBinding binding))
            {
                return;
            }

            if (binding.Button != null)
            {
                if (binding.ClickHandler != null)
                {
                    binding.Button.clicked -= binding.ClickHandler;
                }
                binding.Button.RemoveFromHierarchy();
            }

            blockBindings.Remove(block);
        }

        private void EnsureBlockVisual(Block block)
        {
            if (block == null)
            {
                return;
            }

            if (!blockBindings.TryGetValue(block, out BlockBinding binding))
            {
                Button button = drawer.CreateButton(block);
                button.style.position = Position.Absolute;

                var capturedBlock = block;
                void OnClick()
                {
                    BlockSignals.BlockSelected(capturedBlock);
                }
                button.clicked += OnClick;

                binding = new BlockBinding
                {
                    Button = button,
                    ClickHandler = OnClick
                };

                blockBindings.Add(block, binding);
                Add(button);
            }

            drawer.UpdateButton(binding.Button, block, CurrentZoom);
        }

        private void UpdateBlockLayouts()
        {
            Vector2 scroll = CurrentScroll;
            float zoom = CurrentZoom;

            foreach (var pair in blockBindings)
            {
                Block block = pair.Key;
                Button button = pair.Value.Button;
                if (block == null || button == null)
                {
                    continue;
                }

                Rect rect = block._NodeRect;
                Vector2 viewPos = (rect.position + scroll) * zoom;

                button.style.left = viewPos.x;
                button.style.top = viewPos.y;

                drawer.UpdateButton(button, block, zoom);
            }
        }

        private Vector2 CurrentScroll
        {
            get
            {
                Flowchart flowchart = flowchartContext.Flowchart;
                return flowchart != null ? flowchart.ScrollPos : Vector2.zero;
            }
        }

        private float CurrentZoom
        {
            get
            {
                Flowchart flowchart = flowchartContext.Flowchart;
                float zoom = flowchart != null ? flowchart.Zoom : 1f;
                return Mathf.Approximately(zoom, 0f) ? 1f : zoom;
            }
        }

        public void OnFlowchartChanged(Flowchart previous, Flowchart next)
        {
            RefreshBlocks();
        }

        public void OnWindowPanned()
        {
            UpdateBlockLayouts();
        }

        public void OnScrollWheelMoved()
        {
            UpdateBlockLayouts();
        }

        public void OnBlockSelected(Block block)
        {
            if (block == null)
            {
                return;
            }

            if (blockBindings.TryGetValue(block, out BlockBinding binding))
            {
                drawer.UpdateButton(binding.Button, block, CurrentZoom);
            }
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
            ClearAll();
            this.RemoveFromHierarchy();
        }

        private void ClearAll()
        {
            foreach (var entry in blockBindings)
            {
                UnsubClickHandler(entry.Value);
                entry.Value.Button?.RemoveFromHierarchy();
            }
            blockBindings.Clear();
        }

        private void UnsubClickHandler(BlockBinding binding)
        {
            if (binding.Button != null && binding.ClickHandler != null)
            {
                binding.Button.clicked -= binding.ClickHandler;
            }
        }

        public void OnPreBlockDeletion(IList<Block> blocks)
        {
            for (int i = 0; i < blocks.Count; i++)
            {
                var blockEl = blocks[i];
                RemoveBlock(blockEl);
            }
        }

        public void OnPreBlockDeletion(Block block)
        {
            RemoveBlock(block);
        }
    }

    /// <summary>
    /// Default UITK drawer that produces tinted buttons sized to block text.
    /// </summary>
    public sealed class DefaultBlockDrawerUitk : IBlockDrawerUitk
    {
        private readonly IBlockGraphicsGenerator graphicsGenerator;
        private const float MinWidth = 60f;
        private const float MaxWidth = 260f;
        private const float PaddingX = 18f;
        private const float PaddingY = 10f;
        private const float DefaultHeight = 40f;
        private const float BaseFontSize = 12f;
        private static readonly string BaseClass = "flowchart-block";
        private static readonly string SelectedClass = "flowchart-block--selected";

        public DefaultBlockDrawerUitk()
            : this(new BlockGraphicsGenerator())
        {
        }

        public DefaultBlockDrawerUitk(IBlockGraphicsGenerator graphicsGenerator)
        {
            this.graphicsGenerator = graphicsGenerator ?? throw new ArgumentNullException(nameof(graphicsGenerator));
        }

        public Button CreateButton(Block block)
        {
            var button = new Button
            {
                focusable = false,
                text = SafeBlockName(block)
            };

            button.style.position = Position.Absolute;
            button.style.justifyContent = Justify.Center;
            button.style.alignItems = Align.Center;
            button.style.whiteSpace = WhiteSpace.Normal;
            button.style.paddingLeft = PaddingX;
            button.style.paddingRight = PaddingX;
            button.style.paddingTop = PaddingY * 0.5f;
            button.style.paddingBottom = PaddingY * 0.5f;
            button.style.borderTopLeftRadius = 6;
            button.style.borderTopRightRadius = 6;
            button.style.borderBottomLeftRadius = 6;
            button.style.borderBottomRightRadius = 6;
            button.style.borderLeftWidth = 1;
            button.style.borderRightWidth = 1;
            button.style.borderTopWidth = 1;
            button.style.borderBottomWidth = 1;
            button.style.unityFontStyleAndWeight = FontStyle.Normal;

            button.AddToClassList(BaseClass);

            return button;
        }

        public void UpdateButton(Button button, Block block, float zoom)
        {
            if (button == null || block == null)
            {
                return;
            }

            button.text = SafeBlockName(block);

            UpdateSize();
            void UpdateSize()
            {
                Vector2 unrestrictedSize = button.MeasureTextSize(
                    button.text,
                    float.NaN,
                    MeasureMode.Undefined,
                    float.NaN,
                    MeasureMode.Undefined);

                float totalPaddingX = PaddingX * 2f;
                float unclampedWidth = Mathf.Clamp(unrestrictedSize.x + totalPaddingX, MinWidth, MaxWidth);
                float textWidthConstraint = Mathf.Max(unclampedWidth - totalPaddingX, minTextWidth);

                Vector2 wrappedSize = button.MeasureTextSize(
                    button.text,
                    textWidthConstraint,
                    MeasureMode.AtMost,
                    float.NaN,
                    MeasureMode.Undefined);

                float width = unclampedWidth * zoom;
                float height = Mathf.Max(DefaultHeight, wrappedSize.y + PaddingY) * zoom;

                button.style.width = width;
                button.style.height = height;
            }

            bool isSelected;
            UpdateColors();
            void UpdateColors()
            {
                BlockGraphics graphics = graphicsGenerator.GenerateFor(block);
                Color tint = graphics.tint;
                button.style.backgroundColor = new StyleColor(tint);
                button.style.color = new StyleColor(ChooseTextColor(tint));
                button.style.borderLeftColor = button.style.borderRightColor =
                    button.style.borderTopColor = button.style.borderBottomColor =
                        new StyleColor(new Color(0f, 0f, 0f, 0.4f));

                isSelected = block.IsSelected && !block.IsControlSelected;
                button.EnableInClassList(SelectedClass, isSelected);
                if (isSelected)
                {
                    button.style.borderLeftColor = button.style.borderRightColor =
                        button.style.borderTopColor = button.style.borderBottomColor = Color.white;
                }
            }

            UpdateFont(button, zoom);
            void UpdateFont(Button button, float zoom)
            {
                button.transform.scale = new Vector3(zoom, zoom, 1f);
                button.style.fontSize = Mathf.RoundToInt(BaseFontSize);
            }
        }

        private static readonly float minTextWidth = 1f;

        private static string SafeBlockName(Block block)
        {
            string result = "New Block";
            if (block != null)
            {
                result = block.BlockName;
                if (result.Length > maxBlockNameLength)
                {
                    result = result[..maxBlockNameLength];
                }
            }

            return result;
        }

        private static readonly int maxBlockNameLength = 50;

        private static Color ChooseTextColor(Color background)
        {
            float luminance = 0.299f * background.r + 0.587f * background.g + 0.114f * background.b;
            return luminance >= 0.5f ? Color.black : Color.white;
        }
    }

    /// <summary>
    /// Lightweight list pool to avoid allocations when diffing block collections.
    /// </summary>
    internal static class ListPool<T>
    {
        private static readonly Stack<List<T>> pool = new();

        public static List<T> Get()
        {
            return pool.Count > 0 ? pool.Pop() : new List<T>();
        }

        public static void Release(List<T> list)
        {
            list.Clear();
            pool.Push(list);
        }

        public struct DisposableList : IDisposable
        {
            private List<T> list;

            public DisposableList(List<T> list)
            {
                this.list = list;
            }

            public static implicit operator List<T>(DisposableList disposable) => disposable.list;

            public void Dispose()
            {
                if (list != null)
                {
                    Release(list);
                    list = null;
                }
            }
        }

        public static DisposableList Get(out List<T> list)
        {
            list = Get();
            return new DisposableList(list);
        }
    }
}