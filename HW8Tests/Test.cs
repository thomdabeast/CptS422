using CS422;
using NUnit.Framework;
using System;

namespace HW8Tests
{
	[TestFixture()]
	public class Test
	{
		[Test()]
		public void TestCase()
		{
			WebServer.AddService(new FilesWebService());
			WebServer.Start(3000, 10);
		}
	}
}
