using CS422;
using System;

namespace MasterProgram
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			WebServer.AddService(new DemoService());

			WebServer.Start(3000, 10);

			Console.ReadKey();

			WebServer.Stop();
		}
	}
}
