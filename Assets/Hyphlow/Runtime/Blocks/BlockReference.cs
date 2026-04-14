namespace AtMycelia.Hyphlow
{
    /// <summary>
    /// A simple struct wrapping a reference to an Amanita Block. Allows for BlockReferenceDrawer. 
    /// This is the recommended way to directly reference an Amanita block in external C# scripts,
    /// as it will give you an inspector field that gives a drop down of all the blocks on a 
    /// Flowchart, in a similar way to what you would expect from selecting a Block on a Command.
    /// 
    /// If you want to showup in the Callers section of the Block, ensure your MonoBehaviours 
    /// that have these also implement IBlockCaller.
    /// </summary>
    [System.Serializable]
    public struct BlockReference
    {
        public Block block;

        public readonly void Execute()
        {
            if (block != null)
                block.StartExecution();
        }
    }
}