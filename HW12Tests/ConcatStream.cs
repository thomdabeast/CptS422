using System;
using System.IO;

namespace CS422 
{
    public class ConcatStream : Stream
    {
        Stream first, second;
        long length, position;
        bool expandable, givenLength;
        
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
			catch (NotSupportedException e)
			{
				throw new ArgumentException();
			}

			// Does the second Stream use the Length property?
			try
			{
				var test = second.Length;
			}
			catch (NotSupportedException e2)
			{
				expandable = true;
			}

			// Use the length they gave us
            if(length != -1)
            {// Second stream doesn't support Length
				givenLength = true;
                this.length = length;
            }
            else
            {
				if (expandable)
				{
					this.length = -1;
				}
				else
				{
					this.length = first.Length + second.Length;
				}
            }

            this.first = first;
            this.second = second;

            Position = first.Position = 0;
        }

		public override long Length 
		{ 
			get
			{
				if (length == -1)
				{
					throw new NotSupportedException();
				}

				return (givenLength) ? length : first.Length + second.Length;
			} 
		}

        public override long Position 
        {
			get { return position; }
			set 
			{ 
				position = value; 
			}
        }

        public override void SetLength(long l)
        {
			if (CanSeek && CanWrite)
			{
				if (l < length)
				{
					if (l < first.Length)
					{
						second.SetLength(0);
						                 
						first.SetLength(l);

						length = first.Length;
					}
					else if (l > first.Length)
					{
						second.SetLength(l - first.Length);

						length = l;
					}
				}
				else if (l > length)
				{
					second.SetLength(l - first.Length);

					length = first.Length + second.Length;
				}
			}
			else
			{
				throw new NotSupportedException();
			}
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
				if (!expandable && Position >= Length)
				{
					break;
				}
                // Read from the first stream
                if(Position < first.Length)
                {
                    buffer[i] = (byte)first.ReadByte();
                }
                else
                {
					if (second.Position != Position - first.Length)
					{// The second stream should be at the right position
						if (!second.CanSeek)
						{
							throw new ArgumentOutOfRangeException();
						}
						second.Seek(Position - first.Length, SeekOrigin.Begin);
					}

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
			if (CanWrite)
			{
				for (int i = offset; i < count + offset; i++, Position++)
				{
					if (!expandable && Position >= Length)
					{
						break;
					}

					if (Position < first.Length)
					{
						first.WriteByte(buffer[i]);
					}
					else
					{// We're going to use the second stream
					 // Does the stream support seeking?
						if (second.CanSeek)
						{
							second.Seek(Position - first.Length, SeekOrigin.Begin);
						}
						else if (second.Position != Position - first.Length)
						{// The second stream should be at the right position
							throw new ArgumentOutOfRangeException();
						}

						// Write to second stream
						second.WriteByte(buffer[i]);
					}
				}
			}
			else
			{
				throw new NotSupportedException();
			}
		}

        public override void Flush()
        {
            first.Flush();
            second.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
			if (CanSeek)
			{
				switch (origin)
				{
					case SeekOrigin.Begin:
						first.Seek(offset, origin);
						Position = offset;
						if (offset > first.Length) second.Seek(offset - first.Length, origin);
						else second.Seek(0, SeekOrigin.Begin);
						break;
					case SeekOrigin.Current:
						Position += offset;
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
						Position = Length + offset;
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
				return Position;
			}
			else
			{
				throw new NotSupportedException();
			}
        }
    }
}
