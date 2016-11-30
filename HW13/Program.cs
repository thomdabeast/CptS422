using System;

namespace CS422
{
	public class Program
	{
		const int PORT = 3000;

		public Program()
		{
			
		}

		public static void Main(string[] args)
		{
			WebServer.AddService(new FilesWebService(StandardFileSystem.Create("root")));

			Console.WriteLine("Starting server on port {0}", PORT);

			WebServer.Start(PORT, 10);

			Console.ReadKey();

		}
	}
}
