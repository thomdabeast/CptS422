using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace CS422
{
	public class WebRequest
	{
		string method, uri, http_version;
		Dictionary<string, string> headers;
		Stream body;
		NetworkStream network;

		public WebRequest(ref NetworkStream network, string method, string uri, string httpversion, Dictionary<string, string> headers, Stream body)
		{
			this.network = network;
			this.method = method;
			this.uri = uri;
			http_version = httpversion;
			this.headers = headers;
			this.body = body;
		}

		public string Method
		{
			get { return method; }
		}

		public string URI
		{
			get { return uri; }
		}

		public string HTTPVersion
		{
			get { return http_version; }
		}

		public Stream Body
		{
			get { return body; }
		}

		public void WriteNotFoundResponse(string pageHTML)
		{
			StringBuilder response = new StringBuilder();

			// Version
			response.Append(http_version);
			response.Append(" ");

			// Status code
			response.Append(404);
			response.Append(" ");

			// Reason
			response.Append("Not Found\r\n");

			// Headers
			response.Append("Content-Type: text/html\r\n");
			response.Append("Content-Length: " + pageHTML.Length + "\r\n");

			// Body
			response.Append("\r\n");
			response.Append(pageHTML);

			byte[] res = Encoding.ASCII.GetBytes(response.ToString());

			network.Write(res, 0, res.Length);
		}

		public bool WriteHTMLResponse(string htmlString)
		{
			StringBuilder response = new StringBuilder();

			// Version
			response.Append(http_version);
			response.Append(" ");

			// Status code
			response.Append(200);
			response.Append(" ");

			// Reason
			response.Append("OK\r\n");

			// Headers
			response.Append("Content-Type: text/html\r\n");
			response.Append("Content-Length: " + htmlString.Length + "\r\n");

			// Body
			response.Append("\r\n");
			response.Append(htmlString);

			byte[] res = Encoding.ASCII.GetBytes(response.ToString());

			network.Write(res, 0, res.Length);

			return true;
		}
	}
}
