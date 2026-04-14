using System;
using UnityEngine;
using UnityEngine.UIElements;
using UitkLabel = UnityEngine.UIElements.Label;

namespace AtMycelia.Hyphlow.EditorUtils.FcWindow
{
    /// <summary>
    /// A button representing a Block in the flowchart. Displays the Block's name and changes 
    /// appearance based on things like whether the Block is selected, how long its name is,
    /// etc. The button itself doesn't know anything about the Block's position in the graph, or
    /// its connections to other Blocks.
    /// </summary>
    public class BlockButton : VisualElement, IDisposable
    {
        public static readonly string BaseClass = "flowchartBlock";
        public static readonly string SelectedClass = "flowchartBlockSelected";
        private static readonly string EventHandlerHiddenClass = "block-event-handler-label-hidden";

        private const float MinWidth = 60f;
        private const float MaxWidth = 280f;
        private const float PaddingX = 18f;
        private const float PaddingY = 10f;
        private const float DefaultHeight = 40f;
        private const float BaseFontSize = 16f;

        private static readonly float minTextWidth = 1f;
        private static readonly int maxBlockNameLength = 50;

        private static readonly Color GradientTop = new Color(1f, 1f, 1f, 0.18f);
        private static readonly Color GradientBottom = new Color(0f, 0f, 0f, 0.25f);

        private readonly IBlockGraphicsGenerator graphicsGenerator;

        public BlockButton(IBlockGraphicsGenerator graphicsGenerator)
        {
            this.graphicsGenerator = graphicsGenerator ?? new BlockGraphicsGenerator();
        }

        public void Initialize(Block block, VisualTreeAsset blockTemplate, StyleSheet baseStyleSheet,
            StyleSheet selectedStyleSheet)
        {
            Validate(block, blockTemplate, baseStyleSheet, selectedStyleSheet);
            InitializeInternal(block, blockTemplate, baseStyleSheet, selectedStyleSheet, true);

            if (block != null)
            {
                UpdateVisuals(block, 1f);
            }
        }

        private void Validate(Block block, VisualTreeAsset blockTemplate, StyleSheet baseStyleSheet,
            StyleSheet selectedStyleSheet)
        {
            if (block == null)
            {
                throw new ArgumentNullException(nameof(block));
            }

            if (blockTemplate == null)
            {
                throw new ArgumentNullException(nameof(blockTemplate));
            }

            if (baseStyleSheet == null)
            {
                throw new ArgumentNullException(nameof(baseStyleSheet));
            }

            if (selectedStyleSheet == null)
            {
                throw new ArgumentNullException(nameof(selectedStyleSheet));
            }
        }

        private void InitializeInternal(Block block, VisualTreeAsset blockTemplate, StyleSheet baseStyleSheet,
            StyleSheet selectedStyleSheet, bool enableSubscriptions)
        {
            _block = block;
            _templateInstance = blockTemplate.Instantiate();
            _templateInstance.name = block.BlockName;

            _clickable = _templateInstance.Q<Button>("ClickableArea");
            _nameLabel = _templateInstance.Q<UitkLabel>("BlockName");
            _eventHandlerLabel = _templateInstance.Q<UitkLabel>("EventHandlerName");
            Validate(_clickable, _nameLabel, _eventHandlerLabel);

            _clickable.focusable = false;

            Add(_templateInstance);
            ApplyStyles(baseStyleSheet, selectedStyleSheet);

            if (enableSubscriptions)
            {
                ToggleSubs(true);
            }
        }

        private Block _block;
        private VisualElement _templateInstance;
        private Button _clickable;
        private UitkLabel _nameLabel;
        private UitkLabel _eventHandlerLabel;
        private bool sizeRefreshQueued;

        private void Validate(Button clickable, UitkLabel nameLabel, UitkLabel eventHandlerLabel)
        {
            if (clickable == null)
            {
                throw new ArgumentNullException(nameof(clickable));
            }
            if (nameLabel == null)
            {
                throw new ArgumentNullException(nameof(nameLabel));
            }
            if (eventHandlerLabel == null)
            {
                throw new ArgumentNullException(nameof(eventHandlerLabel));
            }
        }

        public VisualElement InputTarget => _clickable != null ? _clickable : this;

        public void SetPickingMode(PickingMode pickingMode)
        {
            this.pickingMode = pickingMode;
            if (_clickable != null)
            {
                _clickable.pickingMode = pickingMode;
            }
        }

        public Vector2 MeasureTextSize(string text, float width, MeasureMode widthMode, 
            float height, MeasureMode heightMode)
        {
            if (_nameLabel == null)
            {
                return Vector2.zero;
            }

            _nameLabel.text = text;
            return _nameLabel.MeasureTextSize(text, width, widthMode, height, heightMode);
        }

        public void UpdateVisuals(Block block, float zoom)
        {
            if (block == null)
            {
                return;
            }

            _block = block;
            UpdateBlockName();
            UpdateEventHandlerName();
            UpdateFont(zoom);
            UpdateSize();
            UpdateColors();
            UpdateSelectionClass();
        }

        private void UpdateBlockName()
        {
            if (_nameLabel == null || _block == null)
            {
                return;
            }

            _nameLabel.text = SafeBlockName(_block);
        }

        private void UpdateEventHandlerName()
        {
            if (_eventHandlerLabel == null || _block == null)
            {
                return;
            }

            var handler = _block._EventHandler;
            if (handler == null)
            {
                _eventHandlerLabel.text = string.Empty;
                _eventHandlerLabel.EnableInClassList(EventHandlerHiddenClass, true);
                return;
            }

            var info = EventHandlerEditor.GetEventHandlerInfo(handler.GetType());
            string handlerName = handler.DisplayNameAboveBlock;

            _eventHandlerLabel.text = string.Format(_eventHandlerLabelFormat, handlerName);
            _eventHandlerLabel.EnableInClassList(EventHandlerHiddenClass, string.IsNullOrEmpty(handlerName));
        }

        private static readonly string _eventHandlerLabelFormat = "[{0}]";

        private void ApplyStyles(StyleSheet baseStyleSheet, StyleSheet selectedStyleSheet)
        {
            if (_nameLabel != null)
            {
                _nameLabel.style.unityFontStyleAndWeight = FontStyle.Normal;
            }

            if (_clickable == null)
            {
                return;
            }

            _clickable.AddToClassList(BaseClass);
            _clickable.AddToClassList(SelectedClass);
            _clickable.EnableInClassList(SelectedClass, false);

            GradientDrawer.AttachVerticalGradient(_clickable, GradientTop, GradientBottom);
        }

        private void UpdateFont(float zoom)
        {
            if (_nameLabel != null)
            {
                _nameLabel.style.fontSize = Mathf.RoundToInt(BaseFontSize);
            }

            style.transformOrigin = new TransformOrigin(0f, 0f, 0f); // To make sure the connections render right
            transform.scale = new Vector3(zoom, zoom, 1f);
        }

        private void UpdateSize()
        {
            if (_nameLabel == null || _block == null)
            {
                return;
            }

            if (panel == null)
            {
                QueueSizeRefresh();
                return;
            }

            Vector2 unrestrictedSize = _nameLabel.MeasureTextSize(
                _nameLabel.text,
                float.NaN,
                MeasureMode.Undefined,
                float.NaN,
                MeasureMode.Undefined);

            if (IsInvalidSize(unrestrictedSize))
            {
                QueueSizeRefresh();
                return;
            }

            float totalPaddingX = PaddingX * 2f;
            float unclampedWidth = Mathf.Clamp(unrestrictedSize.x + totalPaddingX, MinWidth, MaxWidth);
            float textWidthConstraint = Mathf.Max(unclampedWidth - totalPaddingX, minTextWidth);

            Vector2 wrappedSize = _nameLabel.MeasureTextSize(_nameLabel.text, textWidthConstraint, MeasureMode.AtMost,
                float.NaN, MeasureMode.Undefined);

            if (IsInvalidSize(wrappedSize))
            {
                QueueSizeRefresh();
                return;
            }

            float width = unclampedWidth;
            float height = Mathf.Max(DefaultHeight, wrappedSize.y + PaddingY);

            if (IsInvalidNumber(width) || IsInvalidNumber(height))
            {
                QueueSizeRefresh();
                return;
            }

            ApplySize(width, height);

            Rect newNodeRect = _block._NodeRect;
            newNodeRect.width = width;
            newNodeRect.height = height;
            _block._NodeRect = newNodeRect;
        }

        private void QueueSizeRefresh()
        {
            if (sizeRefreshQueued || _block == null)
            {
                return;
            }

            sizeRefreshQueued = true;
            schedule.Execute(() =>
            {
                if (_block == null)
                {
                    return;
                }

                sizeRefreshQueued = false;
                UpdateSize();
            }).ExecuteLater(1);
        }

        private static bool IsInvalidSize(Vector2 size)
        {
            return IsInvalidNumber(size.x) || IsInvalidNumber(size.y);
        }

        private static bool IsInvalidNumber(float value)
        {
            return float.IsNaN(value) || float.IsInfinity(value);
        }

        private void ApplySize(float width, float height)
        {
            style.width = width;
            style.height = height;

            if (_templateInstance != null)
            {
                _templateInstance.style.width = width;
                _templateInstance.style.height = height;
            }

            if (_clickable != null)
            {
                _clickable.style.width = width;
                _clickable.style.height = height;
            }
        }

        private void UpdateColors()
        {
            if (_block == null)
            {
                return;
            }

            BlockGraphics graphics = graphicsGenerator.GenerateFor(_block);
            Color tint = graphics.tint;
            BackgroundColor = tint;
            TextColor = ChooseTextColor(tint);
        }

        private void UpdateSelectionClass()
        {
            if (_clickable == null || _block == null)
            {
                return;
            }

            bool isSelected = _block.IsSelected && !_block.IsControlSelected;
            _clickable.EnableInClassList(SelectedClass, isSelected);
        }

        private void ToggleSubs(bool on)
        {
            if (on)
            {
                Clicked += OnClicked;
            }
            else
            {
                Clicked -= OnClicked;
            }
        }

        private void OnClicked()
        {
            if (_clickable == null)
            {
                return;
            }

            _clickable.schedule.Execute(UpdateSelectionClass).ExecuteLater(1);
        }

        public virtual string Text
        {
            get => _nameLabel != null ? _nameLabel.text : string.Empty;
            set
            {
                if (_nameLabel != null)
                {
                    _nameLabel.text = value;
                }
            }
        }

        public virtual Color TextColor
        {
            get => _nameLabel != null ? _nameLabel.style.color.value : default;
            set
            {
                if (_nameLabel != null)
                {
                    _nameLabel.style.color = value;
                }
            }
        }

        public virtual Color BackgroundColor
        {
            get => _clickable != null ? 
                _clickable.style.backgroundColor.value : 
                default;
            set
            {
                if (_clickable != null)
                {
                    _clickable.style.backgroundColor = value;
                }
            }
        }

        public event Action Clicked
        {
            add
            {
                if (_clickable != null)
                {
                    _clickable.clicked += value;
                }
            }
            remove
            {
                if (_clickable != null)
                {
                    _clickable.clicked -= value;
                }
            }
        }

        public void Dispose()
        {
            ToggleSubs(false);
            _templateInstance?.RemoveFromHierarchy();
            RemoveFromHierarchy();
            _clickable = null;
            _nameLabel = null;
            _eventHandlerLabel = null;
            _block = null;
        }

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

        private static Color ChooseTextColor(Color background)
        {
            float luminance = 0.299f * background.r + 0.587f * background.g + 0.114f * background.b;
            return luminance >= 0.5f ? 
                Color.black : 
                Color.white;
        }
    }
}