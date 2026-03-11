namespace AtMycelia.SaveSys
{
    public class BaseDecryptionRequest
    {
        public byte[] RawBytes { get; set; }
        public bool WrittenAsPlainText { get; set; }
        public string CompletionMarker { get; set; }
    }
}