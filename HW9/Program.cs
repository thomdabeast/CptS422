using CS422;

using System;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace HW9
{
	public class Program
	{
		public static void Main(string[] args)
		{
			WebServer.AddService(new FilesWebService(StandardFileSystem.Create("root")));

			WebServer.Start(3000, 10);


			Console.ReadKey();
		}
	}
}
