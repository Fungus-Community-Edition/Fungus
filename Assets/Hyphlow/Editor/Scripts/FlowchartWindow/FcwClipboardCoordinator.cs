namespace AtMycelia.Hyphlow.EditorUtils.FcWindow
{
    internal sealed class FcwClipboardCoordinator
    {
        public AmanitaClipboard EnsureClipboard(AmanitaClipboard current, IFlowchartHostCore host)
        {
            return current ?? new AmanitaClipboard(host);
        }

        public BlockClipboard GetBlockClipboard(AmanitaClipboard current)
        {
            return current?.BlockClipboard;
        }

        public AmanitaClipboard SetBlockClipboard(AmanitaClipboard current, BlockClipboard blockClipboard)
        {
            CommandClipboard commandClipboard = current?.CommandClipboard ?? new CommandClipboard();
            return new AmanitaClipboard(blockClipboard, commandClipboard);
        }

        public bool HasClipboard(AmanitaClipboard current)
        {
            return current?.BlockClipboard != null && current.BlockClipboard.HasEntries;
        }
    }
}