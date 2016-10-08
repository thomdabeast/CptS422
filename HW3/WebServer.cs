using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CS422
{
	public class WebServer
	{

		public WebServer()
		{
		}

		public static bool Start(int port, string format)
		{
			TcpListener listener = new TcpListener(IPAddress.Any, port);
			listener.Start();

			try
			{
				// Block and wait for client connection
				TcpClient client = listener.AcceptTcpClient();

				using (NetworkStream stream = client.GetStream())
				{
					StringBuilder request = new StringBuilder();
					int buffer;
					int part = 0;
					string url = "";

					// Read byte by byte
					while((buffer = stream.ReadByte()) >= 0)
					{
						char letter = Encoding.ASCII.GetChars(new byte[] { (byte)buffer })[0];
						request.Append(letter);

						// Read line by line
						if(letter == '\n')
						{

							switch(part)
							{
								// METHOD URL HTTPVERSION
								case 0:
								string[] first = request.ToString().Split(' ');

								// Invalid request
								if(first.Length != 3 && (first[0] != "GET" && first[0] != "POST") || 
									(first[1] == "") ||
									(first[2] != "HTTP/1.1\r\n"))
								{
									return false;
								}	
								else 
								{
									url = first[1]; // Main header is valid, save URL
								}
								break;

								// Additional headers
								default:
								int seperator = request.ToString().IndexOf(": ");
								
								// We have reached the end of the request, don't need the body...
								if(seperator == -1)
								{
									// Create the response
									string response = 
										string.Format(format, 123456, DateTime.Now, url);
									byte[] buf = Encoding.ASCII.GetBytes(response);

									// Send the response
									stream.Write(buf, 0, buf.Length);
									
									// End the stream
									stream.Dispose();
									return true;
								}
								// To be used for future implementations.
								//else
								//{
								//	string name = request.ToString().Substring(0, seperator);
								//	string value = request.ToString()
								//		.Substring(seperator+2);
								//}
								break;
							}

							// Clear the string builder to get ready for the next part of the header.
							request.Clear();
							part++;
						}
					}
							
				}
			}
			catch (Exception e)
			{
				return false;
			}

			return true;
		}
	}
}

