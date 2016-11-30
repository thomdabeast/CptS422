using System;

namespace CS422
{
	public class Program
	{
		public Program()
		{
		}

		public static void Main(string[] args)
		{
			WebServer.AddService(new DemoService());

			WebServer.Start(3000, 10);
		}
	}
}
