using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UIToolkitLabel = UnityEngine.UIElements.Label;
using Amanita.VScripting;
using Amanita.VScripting.EditorUtils;
using Collections;

namespace Amanita.EditorUtils
{
    public class SearchPanel : IDisposable
    {
        // Settings
        protected static readonly int resultItemHeight = 20, resultListHeight = 200,
            searchFieldMarginBottom = 4;

        public SearchPanel(Flowchart toSearchFor)
        {
            flowchart = toSearchFor;
            if (flowchart != null)
            {
                AllBlocks = toSearchFor.GetComponents<Block>();
            }
            Root = new VisualElement();

            BuildUI();
            RebindResults();
        }

        protected Flowchart flowchart;
        protected IList<Block> AllBlocks
        {
            get => _allBlocks;
            set
            {
                _allBlocks.Clear();
                if (value != null)
                {
                    _allBlocks.AddRange(value);
                }
            }
        }
        protected IList<Block> _allBlocks = new List<Block>();
        public VisualElement Root { get; }

        protected virtual void BuildUI()
        {
            PrepSearchField();
            void PrepSearchField()
            {
                searchField = new ToolbarSearchField();
                searchField.name = SearchFieldName;
                searchField.style.marginBottom = searchFieldMarginBottom;
                searchField.value = ""; // To avoid certain null ref errors
            }

            PrepResultList();
            void PrepResultList()
            {
                resultList = new ListView
                {
                    itemsSource = new List<Block>(),
                    fixedItemHeight = resultItemHeight,
                    selectionType = SelectionType.Single,
                    style = { height = resultListHeight }
                };
            }

            ListenForUiEvents();

            AddUIToRoot();
        }

        public static readonly string SearchFieldName = "FlowchartSearchField";

        protected ToolbarSearchField searchField;
        protected ListView resultList; // Shows Block Names

        protected virtual void ListenForUiEvents()
        {
            searchField.RegisterValueChangedCallback(OnSearchFieldQueryChanged);
            searchField.RegisterCallback<FocusOutEvent>(OnSearchFieldUnfocused);
            resultList.makeItem += MakeItemForResultList;
            resultList.bindItem += BindBlockToResultListItem;
            resultList.selectionChanged += OnResultListSelectionChanged;
        }

        protected virtual void OnSearchFieldQueryChanged(ChangeEvent<string> changeEvent)
        {
            QueryChanged?.Invoke(changeEvent.newValue);
        }

        public event Action<string> QueryChanged = delegate { };

        protected virtual void OnSearchFieldUnfocused(FocusOutEvent evt)
        {
            Debug.Log("Search field lost focus");
            SearchFieldUnfocused(evt);
        }

        public event Action<FocusOutEvent> SearchFieldUnfocused = delegate { };

        protected virtual VisualElement MakeItemForResultList()
        {
            return new UIToolkitLabel();
        }

        protected virtual void BindBlockToResultListItem(VisualElement element, int index)
        {
            if (flowchart == null) // This could happen right as Play Mode starts
            {
                return;
            }

            UIToolkitLabel uitkLabel = (UIToolkitLabel)element;
            AllBlocks = flowchart.GetComponents<Block>();
            IList<Block> blocksInResults = (IList<Block>)resultList.itemsSource;
            Block currentBlock = blocksInResults[index];

            if (currentBlock != null)
            {
                uitkLabel.text = currentBlock.BlockName;
            }
        }

        protected virtual void OnResultListSelectionChanged(IEnumerable<object> blocks)
        {
            var selected = blocks.FirstOrDefault() as Block;
            if (selected != null)
            {
                BlockChosen(selected);
            }
        }
        public event Action<Block> BlockChosen = delegate { };

        protected virtual void AddUIToRoot()
        {
            IList<VisualElement> elementsToRegister = new List<VisualElement>()
            {
                searchField, resultList,
            };

            foreach (var element in elementsToRegister)
            {
                Root.Add(element);
            }
        }

        protected virtual void RebindResults()
        {
            IList<Block> resultsToShow = FilterUtils.FilterBlocks(AllBlocks, Query);

            resultList.itemsSource = (System.Collections.IList)resultsToShow;
            resultList.RefreshItems();
        }

        public virtual int ResultCount
        {
            get
            {
                if (resultList == null)
                    return 0;
                return resultList.childCount;
            }
        }

        public string Query
        {
            get => searchField.value;
            set => searchField.value = value;
        }

        public virtual void Dispose()
        {
            UnregisterUiCallbacks();
            _allBlocks.Clear();
            _allBlocks = null;
            if (Root.parent != null)
                Root.RemoveFromHierarchy();
        }

        protected virtual void UnregisterUiCallbacks()
        {
            searchField.UnregisterValueChangedCallback(OnSearchFieldQueryChanged);
            searchField.UnregisterCallback<FocusOutEvent>(OnSearchFieldUnfocused);
            resultList.makeItem -= MakeItemForResultList;
            resultList.bindItem -= BindBlockToResultListItem;
            resultList.selectionChanged -= OnResultListSelectionChanged;
        }
    }
}