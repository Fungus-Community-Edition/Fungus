using UnityEngine;
using System.Collections.Generic;

namespace AtMycelia.Hyphlow.UI
{
    /// <summary>
    /// Model for Flowchart editor window. Stores information about selected blocks and
    /// commands, scroll position, zoom level, etc.
    /// </summary>
    [System.Serializable]
    public class FlowchartUIModel : IFlowchartUIModel
    {
        [SerializeField] protected List<Block> _selectedBlocks = new List<Block>();
        [SerializeField] protected List<Command> _selectedCommands = new List<Command>();

        [SerializeField]
        private GameObject _owner;

        public virtual GameObject Owner
        {
            get => _owner;
            set => _owner = value;
        }

        [field: SerializeField] public Vector2 ScrollPos { get; set; } = Vector2.zero;

        /// <summary>
        /// Scroll position of Flowchart variables window.
        /// </summary>
        [field: SerializeField] public virtual Vector2 VariablesScrollPos { get; set; }

        /// <summary>
        /// Whether or not to show the variables pane.
        /// </summary>
        [field: SerializeField] public virtual bool VariablesExpanded { get; set; }

        /// <summary>
        /// Zoom level of Flowchart editor window.
        /// </summary>
        [field: SerializeField] public float Zoom { get; set; } = 1f;

        /// <summary>
        /// Height of Command block view in inspector.
        /// </summary>
        [field: SerializeField] public virtual float BlockViewHeight { get; set; } = 400;

        /// <summary>
        /// Scrollable area for Flowchart editor window.
        /// </summary>
        [field: SerializeField] public virtual Rect ScrollViewRect { get; set; }   
        
        public virtual Block SelectedBlock
        {
            get
            {
                if (_selectedBlocks.Count == 0)
                {
                    return null;
                }

                return _selectedBlocks[0];
            }
            set
            {
                ClearSelectedBlocks();
                AddToSelection(value);
            }
        }

        public IList<Block> SelectedBlocks
        {
            get => new List<Block>(_selectedBlocks);
            set
            {
                ClearSelectedBlocks();
                AddRangeToSelection(value);
            }
        }

        public Command SelectedCommand
        {
            get
            {
                if (_selectedCommands.Count == 0)
                {
                    return null;
                }

                return _selectedCommands[0];
            }
            set
            {
                ClearSelectedCommands();
                AddToSelection(value);
            }
        }

        public IList<Command> SelectedCommands
        {
            get => new List<Command>(_selectedCommands);
            set
            {
                ClearSelectedCommands();
                AddRangeToSelection(value);
            }
        }

        public virtual void ClearSelectedBlocks()
        {
            int amountToClear = _selectedBlocks.Count;
            if (amountToClear == 0)
            {
                return;
            }

            Block firstBlock = _selectedBlocks[0];
            IList<Block> blocksToDeselect = new List<Block>(_selectedBlocks);
            foreach (var blockEl in _selectedBlocks)
            {
                if (blockEl == null)
                {
                    continue;
                }
                blockEl.IsSelected = false;
            }
            _selectedBlocks.Clear();

            if (amountToClear > 1)
            {
                BlockSignals.MultiBlocksDeselected(blocksToDeselect);
            }
            else if (amountToClear == 1)
            {
                BlockSignals.BlockDeselected(firstBlock);
            }
        }

        public virtual void ClearSelectedCommands()
        {
            _selectedCommands.Clear();
        }

        public void AddRangeToSelection(IList<Block> toAdd)
        {
            foreach (var blockEl in toAdd)
            {
                // To avoid confusion, we don't want this to be able to trigger MultiBlocksSelected
                // and BlockSelected at the same time in the same call of this func
                AddToSelectionWithoutSignal(blockEl);
            }

            if (toAdd.Count > 0)
            {
                BlockSignals.MultiBlocksSelected(toAdd);
            }
            else if (toAdd.Count == 1)
            {
                BlockSignals.BlockSelected(toAdd[0]);
            }
        }

        public virtual void AddToSelection(Block block)
        {
            if (block != null && !_selectedBlocks.Contains(block))
            {
                AddToSelectionWithoutSignal(block);
                BlockSignals.BlockSelected(block);
            }
        }

        protected virtual void AddToSelectionWithoutSignal(Block block)
        {
            if (block != null && !_selectedBlocks.Contains(block))
            {
                block.IsSelected = true;
                _selectedBlocks.Add(block);
            }
        }

        public virtual void AddRangeToSelection(IList<Command> toAdd)
        {
            foreach (var command in toAdd)
            {
                AddToSelection(command);
            }
        }

        public virtual void AddToSelection(Command toAdd)
        {
            if (!_selectedCommands.Contains(toAdd))
            {
                _selectedCommands.Add(toAdd);
            }
        }

        public virtual void Deselect(Command toRemove)
        {
            _selectedCommands.Remove(toRemove);
        }

        public virtual void Deselect(Block toDeselect)
        {
            DeselectWithoutSignal(toDeselect);
            BlockSignals.BlockDeselected(toDeselect);
        }

        public virtual void DeselectWithoutSignal(Block toDeselect)
        {
            toDeselect.IsSelected = false;
            _selectedBlocks.Remove(toDeselect);
        }

        [field: SerializeField] public bool SelectedCommandsStale { get; set; }

        public virtual bool Contains(Command command)
        {
            return _selectedCommands.Contains(command);
        }

        public virtual bool Contains(Block block)
        {
            return _selectedBlocks.Contains(block);
        }

        public virtual int CommandCount { get { return _selectedCommands.Count; } }
        public virtual int BlockCount { get { return _selectedBlocks.Count; } }
        public virtual void CleanUp()
        {
            // To get rid of unreferenced Blocks and Commands, which should 
            // mean less memory leaks
            _selectedBlocks.RemoveAll(item => item == null);
            _selectedCommands.RemoveAll(item => item == null);
        }

    }
}
