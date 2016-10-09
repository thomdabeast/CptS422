using CS422;
using NUnit.Framework;
using System.IO;

namespace HW5
{
    [TestFixture()]
    class ConcatStreamTests
    {
        [Test()]
        public void ConcatStreamTest()
        {
            byte[] buffer1 = new byte[10];
            byte[] buffer2 = new byte[10];

            ConcatStream stream = new ConcatStream(new MemoryStream(buffer1), new NoSeekMemoryStream(buffer2));

            stream.Read(new byte[20], 0, 20);

            stream.Seek(0, SeekOrigin.Begin);
        }
    }
}
