using Amanita;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UIToolkitLabel = UnityEngine.UIElements.Label;

namespace Amanita.EditorUtils
{
    public class SearchPanel
    {
        public VisualElement Root { get; }
        public string Query => searchField.value;

        private ToolbarSearchField searchField;
        private ListView resultList;
        private Block[] allBlocks;
        private Flowchart flowchart;

        public SearchPanel(Flowchart fc)
        {
            flowchart = fc;
            allBlocks = fc.GetComponents<Block>();
            Root = new VisualElement();

            BuildUI();
            RebindResults();
        }

        void BuildUI()
        {
            // 1) SearchField
            searchField = new ToolbarSearchField();
            searchField.style.marginBottom = 4;
            searchField.RegisterValueChangedCallback(_ => RebindResults());
            Root.Add(searchField);

            // 2) ListView showing block names
            resultList = new ListView
            {
                makeItem = () => new UIToolkitLabel(),
                bindItem = (ve, i) => ((UIToolkitLabel)ve).text = flowchart.GetComponents<Block>()[i].BlockName,
                itemsSource = new List<Block>(),
                fixedItemHeight = 20,
                selectionType = SelectionType.Single,
                style =
                {
                    flexGrow = 1,
                    height   = 200
                }
            };
            resultList.selectionChanged += (blocks) =>
            {
                var selected = blocks.FirstOrDefault() as Block;
                if (selected != null)
                {
                    BlockChosen(selected);

                }
            };
            Root.Add(resultList);
        }

        public event Action<Block> BlockChosen = delegate { };

        void RebindResults()
        {
            // 1) Get filtered list
            IList<Block> filtered = FilterUtils.FilterBlocks(allBlocks, Query);

            // 2) Update ListView
            resultList.itemsSource = (System.Collections.IList)filtered;
            resultList.RefreshItems();
        }
    }
}