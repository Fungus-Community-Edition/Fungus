using Amanita.SaveSys;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SaveSystemTests
{
    public class TestSaveReader : SaveReader
    {
        protected override Task<byte[]> ReadAllBytesAsync(string filePath, CancellationToken cancelToken)
        {
            // Synchronous read for test stability
            return Task.FromResult(File.ReadAllBytes(filePath));
        }
    }
}