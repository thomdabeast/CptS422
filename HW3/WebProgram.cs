using CS422;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class WebProgram
{
	public static void Main(string[] args)
	{
		int port = 4200;
		const string DefaultTemplate = "HTTP/1.1 200 OK\r\n" +
			"Content-Type: text/html\r\n" + "\r\n\r\n" +
			"<html>ID Number: {0}<br>" + "DateTime.Now: {1}<br>" + 
			"Requested URL: {2}</html>";

		Console.WriteLine("Starting web server on port {0}", port); 
		
		// Testing stuff
		if(args.Length > 0 && args[0] == "test")
		{
			Thread client = new Thread(() => {
				TcpClient c = new TcpClient("localhost", 4200);
			
				using(NetworkStream stream = c.GetStream())
				{
					// Create request
					byte[] buffer = Encoding.ASCII.GetBytes(
						MakeRequest("GET", "/", "HTTP/2"));
					stream.Write(buffer, 0, buffer.Length);
				}
			});

			client.Start();
		}


		WebServer.Start(port, DefaultTemplate);
		
		Console.WriteLine("Web server closing...");
	}
		
	public static string MakeRequest(string method, string url, string httpversion)
	{
		return string.Format("{0} {1} {2}\r\n\r\n", method, url, httpversion);
	}
}
