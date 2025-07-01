namespace Amanita.SaveSys
{
    /// <summary>
    /// For displaying save slot metadata such as the time stamp, slot number, etc.
    /// </summary>
    public interface ISaveSlotUIView
    {
        ISaveMetaData Meta { get; set; }
        void Refresh();
    }
}