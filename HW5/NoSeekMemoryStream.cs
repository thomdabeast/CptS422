using System.IO;
using System;

namespace CS422
{
    public class NoSeekMemoryStream : MemoryStream
    {
        public NoSeekMemoryStream(byte[] buffer) : base(buffer)
        {
        }

        public NoSeekMemoryStream(byte[] buffer, int offset, int count) : base(buffer, offset, count)
        {
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        //public override int Read(byte[] buffer, int offset, int count)
        //{
            
        //}

        //public override void Write(byte[] buffer, int offset, int count)
        //{

        //}
        
    }
}
