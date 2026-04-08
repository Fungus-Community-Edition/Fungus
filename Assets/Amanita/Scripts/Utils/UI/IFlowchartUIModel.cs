using UnityEngine;
using System.Collections.Generic;

namespace AtMycelia.Hyphlow.UI
{
    public interface IFlowchartUIModel
    {
        IList<Block> SelectedBlocks { get; set; }
        IList<Command> SelectedCommands { get; set; }

        Vector2 ScrollPos { get; set; }
        Vector2 VariablesScrollPos { get; set; }
        bool VariablesExpanded { get; set; }

        float Zoom { get; set; }
        float BlockViewHeight { get; set; }
        Rect ScrollViewRect { get; set; }

        Block SelectedBlock { get; set; }
        Command SelectedCommand { get; set; }

        void ClearSelectedBlocks();
        void ClearSelectedCommands();

        void AddRangeToSelection(IList<Block> toAdd);
        void AddToSelection(Block block);

        void AddRangeToSelection(IList<Command> toAdd);
        void AddToSelection(Command command);

        void Deselect(Block toDeselect);
        void Deselect(Command toDeselect);

        void CleanUp();
    }
}
