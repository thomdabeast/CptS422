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
        
        public static void Main(String[] args)
        {
            NoSeekMemoryStream stream = new NoSeekMemoryStream(new byte[]{1, 2, 3});

            byte[] buffer = new byte[3];
            stream.Read(buffer, 0, 1);
            stream.Write(new byte[] {1, 2}, 0, 2);

            foreach(var b in buffer)
            {
                Console.Write(b + " ");
            }

            Console.WriteLine();
        }
    }
}
