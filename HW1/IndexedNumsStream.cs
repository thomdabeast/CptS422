using System;
using System.IO;
using System.Text;

namespace CS422
{
	public class IndexedNumsStream : Stream
	{
		long length;
		long position = 0;
		const int MAX_VALUE = byte.MaxValue + 1;

		public IndexedNumsStream(long l)
		{
			SetLength(l);
		}

		public override bool CanRead
		{
			get
			{
				return true;
			}
		}

		public override bool CanSeek
		{
			get
			{
				return true;
			}
		}

		public override bool CanWrite
		{
			get
			{
				return false;
			}
		}

		public override long Length
		{
			get
			{
				return length;
			}
		}

		public override void SetLength(long value)
		{
			length = (value < 0) ? 0 : value;
		}

		public override long Position
		{
			get { return position; }
			set
			{
				if (value < 0)
				{
					position = 0;
				}
				else if (value > length)
				{
					position = Length;
				}
				else {
					position = value;
				}
			}	
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			for (int i = offset; i < (offset + count); i++)
			{
				// End of stream
				if (i >= buffer.Length)
				{
					return i - offset;
				}

				buffer[i] = (byte)(Position % MAX_VALUE);

				Position++;
			}

			return count;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new Exception("Seek not available in a read-only stream");
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new Exception("Write not available in a read-only stream");
		}

		public override void Flush()
		{
			
		}
	}
}

