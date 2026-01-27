using UnityEngine;
using System.Collections.Generic;

namespace Amanita.VScripting.EditorUtils
{
    public class FlowchartDocument
    {
        public Flowchart Flowchart { get; set; }

        public IList<Block> AllBlocks
        {
            get { return _allBlocks; }
            set
            {
                _allBlocks.Clear();
                if (value == null)
                {
                    return;
                }

                foreach (var block in value)
                {
                    if (block != null)
                    {
                        _allBlocks.Add(block);
                    }
                }
            }
        }

        private readonly IList<Block> _allBlocks = new List<Block>();

        public Block TopmostBlockOverlapping(Vector2 mousePosition)
        {
            if (Flowchart == null)
            {
                return null;
            }

            IList<Block> blocks = _allBlocks;
            if (blocks.Count == 0)
            {
                blocks = Flowchart.GetComponents<Block>();
            }

            Vector2 mousePosInWindowSpace = ToWindowSpace(mousePosition);

            for (int i = blocks.Count - 1; i >= 0; i--)
            {
                var currentBlock = blocks[i];
                if (currentBlock == null)
                {
                    continue;
                }

                Rect windowSpaceRect = currentBlock._NodeRect;
                windowSpaceRect.position += Flowchart.ScrollPos;

                if (windowSpaceRect.Contains(mousePosInWindowSpace))
                {
                    return currentBlock;
                }
            }

            return null;
        }

        public Vector2 ToWindowSpace(Vector2 mousePosition)
        {
            if (Flowchart == null)
            {
                return mousePosition;
            }

            float zoom = Mathf.Approximately(Flowchart.Zoom, 0f) ? 1f : Flowchart.Zoom;
            return mousePosition / zoom;
        }
    }
}