using CS422;
using NUnit.Framework;
using System;

namespace HW13Tests
{
	[TestFixture]
	public class Test
	{
		const int PORT = 3000;

		[Test]
		public void TestCase()
		{
			WebServer.AddService(new FilesWebService(StandardFileSystem.Create("root")));

			Console.WriteLine("Starting server on port {0}", PORT);

			WebServer.Start(PORT, 10);

			Console.ReadKey();
		}
	}
}
