using CS422;
using NUnit.Framework;
using System;
using System.Net.Sockets;

namespace HW12Tests
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

	[TestFixture]
	public class ConcatStreamTests
	{
		[Test]
		public void FirstStreamLengthTest()
		{
			Assert.Throws<ArgumentException>(() => new ConcatStream(
				new NoSeekMemoryStream(new byte[10]), new NoSeekMemoryStream(new byte[10])));
		}
	}
}
