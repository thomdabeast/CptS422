using System;
using CS422;
using NUnit.Framework;

namespace HW1Tests
{
	[TestFixture()]
	public class IndexedNumsStreamTest
	{
		[Test()]
		public void BufferTest()
		{
			IndexedNumsStream indexedNumsStream = new IndexedNumsStream(long.MaxValue);
			byte[] buffer = new byte[byte.MaxValue * 2];

			indexedNumsStream.Read(buffer, 0, buffer.Length);

			for (var b = 0; b < buffer.Length; ++b)
			{
				if (buffer[b] != (b % (byte.MaxValue + 1))) { Console.WriteLine(b); Assert.Fail(); }
			}

			Assert.Pass();
		}

		[Test()]
		public void NegativePositionTest()
		{
			IndexedNumsStream indexedNumsStream = new IndexedNumsStream(long.MaxValue);

			// Set the position to an arbitrary negative number.
			indexedNumsStream.Position = -9999;

			// Should come out to 0.
			Assert.AreEqual(indexedNumsStream.Position, 0);
		}

		[Test()]
		public void PositionGreaterThanLengthTest()
		{
			const long LENGTH = 10;
			IndexedNumsStream indexedNumsStream = new IndexedNumsStream(LENGTH);

			// Set the position above the stream's length.
			indexedNumsStream.Position = LENGTH + 1000;

			// Should come out to the streams length.
			Assert.LessOrEqual(indexedNumsStream.Position, indexedNumsStream.Length);
		}

		[Test()]
		public void NegativeStreamLengthTest()
		{
			// Set streams length to arbitrary negative size.
			IndexedNumsStream indexedNumsStream = new IndexedNumsStream(-4352451245L);

			// Should become 0
			Assert.AreEqual(indexedNumsStream.Length, 0);
		}

		[Test()]
		public void EndOfStreamTest()
		{
			IndexedNumsStream indexedNumStream = new IndexedNumsStream(10L);
			byte[] buffer = new byte[6];

			Console.WriteLine(indexedNumStream.Read(buffer, 0, 4));
			Assert.Pass();
		}
	}
}

