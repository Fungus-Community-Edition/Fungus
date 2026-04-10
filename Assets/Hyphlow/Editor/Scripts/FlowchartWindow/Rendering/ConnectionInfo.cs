namespace AtMycelia.Hyphlow.EditorUtils
{
    public readonly struct ConnectionInfo
    {
        public readonly Block FromBlock;
        public readonly Block ToBlock;
        public readonly bool Highlight;

        public ConnectionInfo(Block fromBlock, Block toBlock, bool highlight)
        {
            FromBlock = fromBlock;
            ToBlock = toBlock;
            Highlight = highlight;
        }
    }
}