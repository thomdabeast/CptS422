using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace CS422
{
	public class WebServer
	{
		const int SECOND = 1000;
		const int FIRST_THRESHOLD = 2048;
		const int SECOND_THRESHOLD = (100 * 1024);

        static List<WebService> services = new List<WebService>();
        static ThreadPool threadPool = new ThreadPool();
		static TcpListener server;

		public static bool Start(int port, int number_threads)
		{
			// Create thread to listen for incoming clients
			new Thread(() =>
			{
                // Init and start server
                server = new TcpListener(IPAddress.Any, port);
                server.Start();

                threadPool.Start((ushort)number_threads);

				while (true)
				{
					TcpClient client;
					try
					{
						client = server.AcceptTcpClient();
					}
					catch
					{
						break;
					}

					// Make thread task function and add it to the ThreadPool
					threadPool.AddTask(() =>
					{
						WebRequest req = BuildRequest(client);

						// Invalid request object, close connection.
						if (null != req)
						{
							// Handle that request
							// Find services that will handle the request URI
							bool foundService = false;
							int longestPrefix = 0;
							WebService serviceToUse = null;

							foreach (var service in services)
							{
								if (req.URI.StartsWith(service.ServiceURI))
								{
									// We'll select the service that has a longer prefix of the request URI and use it
									if (service.ServiceURI.Length > longestPrefix)
									{
										longestPrefix = service.ServiceURI.Length;
										serviceToUse = service;
									}
								}
							}

							if (null == serviceToUse)
							{
								// send 404 reponse
								req.WriteNotFoundResponse("");
							}
							else
							{
								// "Handle" request
								serviceToUse.Handler(req);
							}
						}

						client.Close();
					});
				}
			}).Start();

            return true;
		}

		static WebRequest BuildRequest(TcpClient client)
		{
            WebRequest req = null;
            NetworkStream stream = client.GetStream();
            StringBuilder request = new StringBuilder();
			int buffer, part = 0;
			long bytesRead = 0;
            // Important stuff for the WebRequest
            string method = "", uri = "", httpversion = "";
            Dictionary<string, string> headers = new Dictionary<string, string>();


			// Start the read timeout to timeout in 2 seconds
			System.Timers.Timer readTimeout = new System.Timers.Timer(2 * SECOND);
			readTimeout.AutoReset = false;
			readTimeout.Elapsed += (o, e) => {
				client.Close();
				client.GetStream().Close();
			};

			// Start the request timeout to timeout in 10 seconds
			System.Timers.Timer requestTimeout = new System.Timers.Timer(10 * SECOND);
			requestTimeout.AutoReset = false;
			requestTimeout.Elapsed += (o, e) =>
			{
				client.Close();
				client.GetStream().Close();
			};;

			// Start the timers
			readTimeout.Enabled = true;
			requestTimeout.Enabled = true;

			// Read byte by byte
			try
			{
				while ((buffer = stream.ReadByte()) >= 0)
				{
					// Stop the read timeout
					readTimeout.Stop();

					// Our first data threshold
					if (part == 0 && ++bytesRead >= FIRST_THRESHOLD)
					{
						client.Close();
						return null;
					}

					char letter = Encoding.ASCII.GetChars(new byte[] { (byte)buffer })[0];
					request.Append(letter);

					// Read line by line
					if (letter == '\n')
					{
						switch (part)
						{
							// METHOD URL HTTPVERSION
							case 0:
								string[] first = request.ToString().Split(' ');

								// Invalid request
								if (first.Length != 3 && (first[0] != "GET" && first[0] != "POST") ||
									(first[1] == "") ||
									(first[2] != "HTTP/1.1\r\n"))
								{
									return null;
								}
								else
								{
									method = first[0];
									uri = first[1];
									httpversion = first[2];
								}
								break;

							// Additional headers
							default:
								// CHANGED: Had to change this line to get it to work.
								// string line = request.ToString().Remove(' ');
								string line = request.ToString().Replace(" ", "");
								int seperator = line.IndexOf(":");

								// We have reached the end of the headers, body is starting
								if (seperator == -1)
								{
									// Stop the request timer
									requestTimeout.Stop();

									// Save the body and make WebRequest
									req = new WebRequest(
										ref stream, method, uri, httpversion, headers,
										new ConcatStream(new MemoryStream(0), stream));
									return req;
								}
								else
								{// Get headers
								 // Second data threshold
									if (bytesRead >= SECOND_THRESHOLD)
									{
										client.Close();
										return null;
									}

									string name = line.Substring(0, seperator);
									string value = line.Substring(seperator);

									headers.Add(name, value);
								}
								break;
						}

						// Clear the string builder to get ready for the next part of the header.
						request.Clear();
						part++;

					}
				}
			}
			catch (IOException)
			{
				req = null;
			}

            return req;
        }

		public static void AddService(WebService service)
		{
            services.Add(service);
		}

        public static void Stop()
        {
			services.Clear();
			server.Stop();
            threadPool.Dispose();
        }
	}
}

