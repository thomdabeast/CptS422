using NUnit.Framework;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.IO;

namespace CS422
{
	[TestFixture]
	public class Test
	{
		const int PORT = 3000;

		[SetUp]
		public void Init()
		{
			WebServer.AddService(new DemoService());
			WebServer.Start(PORT, 10);
		}

		[Test]
		public void sendRequestTest()
		{
			using (TcpClient client = new TcpClient("localhost", PORT))
			{
				// Wait until client is connected to the server.
				while (!client.Connected) { }

				using (NetworkStream stream = client.GetStream())
				{
					byte[] message = Encoding.ASCII.GetBytes("GET / HTTP/1.1\r\n\r\n");

					stream.Write(message, 0, message.Length);
				}
			}

			Assert.Pass();
		}

		[Test]
		public void ReadTimeoutTest()
		{
			using (TcpClient client = new TcpClient("localhost", PORT))
			{
				// Wait and let the server close the connection.
				Thread.Sleep(3 * 1000);

				Assert.AreEqual(false, client.Connected);
			}
		}

		[Test]
		public void FirstDataCapTest()
		{
			using (TcpClient client = new TcpClient("localhost", PORT))
			{
				client.GetStream().Write(new byte[4 * 1024], 0, 4 * 1024);
				Thread.Sleep(100);

				Assert.AreEqual(false, client.Connected);
			}
		}

		[TearDown]
		public void Dispose()
		{
			WebServer.Stop();
		}
	}
}
