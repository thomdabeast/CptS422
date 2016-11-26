using NUnit.Framework;
using System;

namespace CS422
{
	[TestFixture()]
	public class NoSeekMemoryStreamTests
	{
		[Test()]
		public void ReadTest()
		{
			byte[] buffer = new byte[1024];
			for (int i = 0; i < 1024; i++)
			{
				buffer[i] = (byte)i;
			}

			NoSeekMemoryStream stream = new NoSeekMemoryStream(buffer);

			byte[] readBytes = new byte[1024];

			stream.Read(readBytes, 0, readBytes.Length);

			for (int j = 0; j < 1024; j++)
			{
				if (buffer[j] != readBytes[j])
				{
					Assert.Fail();
				}
			}
		}

		[Test]
		public void PositionSeekTest()
		{
			NoSeekMemoryStream stream = new NoSeekMemoryStream(new byte[1024]);

			Assert.Throws<NotSupportedException>(() => stream.Position = 5);
		}

		[Test]
		public void SeekMethodTest()
		{
			NoSeekMemoryStream stream = new NoSeekMemoryStream(new byte[1024]);

			Assert.Throws<NotSupportedException>(() => stream.Seek(0, System.IO.SeekOrigin.Begin));
		}

		[Test]
		public void ReadThenSeekTest()
		{
			NoSeekMemoryStream stream = new NoSeekMemoryStream(new byte[1024]);

			stream.Read(new byte[1024], 0, 1024);

			Assert.Throws<NotSupportedException>(() => stream.Position = 0);
		}
	}
}
