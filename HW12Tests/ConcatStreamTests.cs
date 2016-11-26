using NUnit.Framework;
using System;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;

namespace CS422
{
	/*
	 * | ConcatStream unit test that combines two memory streams, reads back all the data in random
	 *		chunk sizes, and verifies it against the original data
	 * | ConcatStream unit test that combines a memory stream as the first and a
	 *		NoSeekMemoryStream as the second, and verifies that all data can be read
	 * | ConcatStream tests that query Length property in both circumstances where it can and cannot
	 * 		be computed without exception
	 * 
	 */
	class NoLengthStream : Stream
	{
		List<byte> buffer = new List<byte>();
		long position;

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
				return true;
			}
		}

		public override long Length
		{
			get
			{
				throw new NotSupportedException();
			}
		}

		public override long Position
		{
			get
			{
				return position;
			}

			set
			{
				position = value;
			}
		}

		public override void Flush()
		{
			throw new NotImplementedException();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			for (int i = offset; i < count + offset; i++)
			{
				buffer[i] = this.buffer[i];
			}

			return count;
		}

		public override int ReadByte()
		{
			return buffer[(int)position++];
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			switch (origin)
			{
				case SeekOrigin.Begin:
					position = offset;
					break;
				case SeekOrigin.Current:
					position += offset;
					break;
				case SeekOrigin.End:
					position = buffer.Count + offset;
					break;
				default:
					break;
			}

			return position;
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			this.buffer.AddRange(buffer);
		}
	}

	[TestFixture]
	public class ConcatStreamTests
	{
		[Test]
		public void FirstStreamLengthTest()
		{
			Assert.Throws<ArgumentException>(() => new ConcatStream(
				new NoLengthStream(), new NoSeekMemoryStream(new byte[10])));
		}

		[Test]
		public void SeekTest()
		{
			ConcatStream stream = new ConcatStream(
				new MemoryStream(new byte[10]), new MemoryStream(new byte[10]));

			Assert.DoesNotThrow(() => stream.Seek(5, SeekOrigin.Begin));

		}

		[Test]
		public void NoSeekTest()
		{

			ConcatStream stream = new ConcatStream(
				new MemoryStream(new byte[10]), new NoSeekMemoryStream(new byte[10]));

			Assert.AreEqual(stream.CanSeek, false);
		}

		[Test]
		public void ReadTest()
		{
			byte[] expected1 = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 },
				expected2 = { 10, 11, 12, 13, 14 };
			byte[] expected = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14 };


			ConcatStream stream = new ConcatStream(
				new MemoryStream(expected1), 
				new MemoryStream(expected2));

			byte[] buffer = new byte[15];

			stream.Read(buffer, 0, buffer.Length);

			Assert.AreEqual(expected, buffer);

		}

		[Test]
		public void LengthTest()
		{
			byte[] buffer1 = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 },
				buffer2 = { 10, 11, 12, 13, 14 };

			ConcatStream stream = new ConcatStream(
				new MemoryStream(buffer1),
				new MemoryStream(buffer2));

			Assert.AreEqual(stream.Length, buffer1.Length + buffer2.Length);
		}

		[Test]
		public void SecondConstructorLengthTest()
		{
			byte[] buffer1 = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 },
				buffer2 = { 10, 11, 12, 13, 14 };
			long expected = 10;

			ConcatStream stream = new ConcatStream(
				new MemoryStream(buffer1),
				new MemoryStream(buffer2), expected);

			Assert.AreEqual(stream.Length, expected);
		}

		[Test]
		public void FixedLengthTest()
		{
			long expected = 10;
			ConcatStream stream = new ConcatStream(
				new MemoryStream(new byte[5]),
			    new NoLengthStream(), expected);

			Assert.AreEqual(stream.Length, expected);
		}

		[Test]
		public void WriteTest()
		{
			ConcatStream stream = new ConcatStream(
				new MemoryStream(new byte[10]),
				new MemoryStream(new byte[20]));
			byte[] expected = new byte[stream.Length];

			for (int i = 0; i < stream.Length; i++)
			{
				expected[i] = (byte)i;
			}

			stream.Write(expected, 0, expected.Length);

			stream.Seek(0, SeekOrigin.Begin);

			byte[] got = new byte[stream.Length];

			stream.Read(got, 0, got.Length);

			Assert.AreEqual(got, expected);
		}

		[Test]
		public void SecondStreamWrongPositionWriteTest()
		{
			NoSeekMemoryStream s = new NoSeekMemoryStream(new byte[20]);
			s.Read(new byte[5], 0, 5);

			ConcatStream stream = new ConcatStream(
				new MemoryStream(new byte[10]),
				s);

			Assert.Throws<ArgumentOutOfRangeException>(() => stream.Write(new byte[40], 0, 40));
		}

		[Test]
		public void ExpandWriteTest()
		{
			MemoryStream expandable = new MemoryStream(100);
			expandable.Write(new byte[10], 0, 10);

			ConcatStream stream = new ConcatStream(new MemoryStream(new byte[10]), expandable);

			byte[] expect = new byte[100];
			for (byte i = 0; i < 100; i++)
			{
				expect[i] = i;
			}

			stream.Write(expect, 0, expect.Length);

			stream.Seek(0, SeekOrigin.Begin);

			byte[] got = new byte[100];

			stream.Read(got, 0, got.Length);

			Assert.AreEqual(got, expect);
			Assert.AreEqual(stream.Length, 100);
		}
	}
}
