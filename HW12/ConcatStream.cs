using System;
using System.IO;

namespace CS422 
{
    public class ConcatStream : Stream
    {
        Stream first, second;
        long length, position;
        bool useLength = false;
        
        public ConcatStream(Stream first, Stream second)
        {
            Construct(first, second);
        }

        public ConcatStream(Stream first, Stream second, long length)
        {
            Construct(first, second, length);
        }
        
        void Construct(Stream first, Stream second, long length = -1)
        {
            try 
            {
                var test = first.Length;
            } 
            catch(NotSupportedException e)
            {
                throw new ArgumentException();
            }
            
            // Does the second Stream use the Length property?
            try
            {
                var test = second.Length;
            }
            catch(NotSupportedException e2)
            {
                useLength = true;
            }

            if(length != -1 && useLength)
            {// Second stream doesn't support Length
                this.length = length + first.Length;
                second.SetLength(length);
            }
            else
            {
                // If length is inputted then use that else we just take both stream's lengths
                this.length = (length == -1) ? first.Length + second.Length : length;
            }

            if(this.Length < 0) this.length = 0;

            this.first = first;
            this.second = second;

            this.Position = this.first.Position = 0;
        }

        public override long Length { get{ return length; } }

        public override long Position 
        {
            get { return position; }
            set
            {
                position = value;

                // Clamp position value
                if(position > Length) 
                {
                    position = Length;
                }
                if(position < 0)
                {
                    position = 0;
                }
            }
        }

        public override void SetLength(long l)
        {
            this.length = l;
        }

        public override bool CanRead
        {
            get { return (first.CanRead && second.CanRead); }
        }
        public override bool CanSeek
        {
            get { return (first.CanSeek && second.CanSeek); }
        }
        public override bool CanWrite
        {
            get { return (first.CanWrite && second.CanWrite); }
        }
        public override bool CanTimeout
        {
            get { return (first.CanTimeout && second.CanTimeout); }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int i = 0;
            for(i = offset; i < count + offset; i++, Position++)
            {
                // Read from the first stream
                if(Position < first.Length)
                {
                    buffer[i] = (byte)first.ReadByte();
                }
                else
                {
//                    if(CanSeek)
//                    {
//                       second.Position = Position - first.Length;
//                  }
//                 else if(second.Position != Position - first.Length)
//                    {
//                       throw new Exception();
//                    }

                    buffer[i] = (byte)second.ReadByte();
                }
            }

            return i - offset;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            /*
             * If a write call cannot be completed then throw an exception, but only do so if the action cannot  
             * possibly be completed. For example, if your ConcatStream position is 32 and the first stream has a
             * length of 20, then you could only write correctly to the second stream if its current position 
             * was exactly 12. In the case when the second stream supports seeking, it’s easy enough to ensure 
             * that, so just seek to the appropriate spot and write in those circumstances. But if the second 
             * stream doesn’t support seeking, then only complete the write if you are at the exac correct 
             * position. Otherwise throw an exception.?
             */
            for(int i = offset; i < count + offset; i++, Position++)
            {
                if(Position < first.Length)
                {
                    first.WriteByte(buffer[i]);
                }
                else
                {// We're going to use the second stream
                    // Does the stream support seeking?
                    if(second.CanSeek)
                    {
                        second.Position = Position - first.Length;
                    }
                    else if(second.Position != Position - first.Length)
                    {// The second stream should be at the right position
                        throw new Exception();
                    }

                    // Write to second stream
                    second.WriteByte(buffer[i]);
                }
            }
        }

        public override void Flush()
        {
            first.Flush();
            second.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if(CanSeek)
            {
                switch(origin)
                {
                    case SeekOrigin.Begin:
                        first.Seek(offset, origin);
                        this.Position = offset;
                        if (offset > first.Length) second.Seek(offset - first.Length, origin);
                    break;
                    case SeekOrigin.Current:
                        this.Position += offset;
                        first.Seek(Position, SeekOrigin.Begin);
                        if (Position > first.Length)
                        {
                            second.Seek(Position - first.Length, SeekOrigin.Begin);
                        }
                        else
                        {
                            second.Seek(0, SeekOrigin.Begin);
                        }
                    break;
                    case SeekOrigin.End:
                        this.Position = Length + offset;
                        first.Seek(Position, SeekOrigin.Begin);
                        if (Position > first.Length)
                        {
                            second.Seek(Position - first.Length, SeekOrigin.Begin);
                        }
                        else
                        {
                            second.Seek(0, SeekOrigin.Begin);
                        }
                    break;
                    default:
                    break;
                }
            }

            return Position;
        }
    }
}
