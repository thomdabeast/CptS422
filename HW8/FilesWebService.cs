using System;
using System.IO;
using System.Text;

namespace CS422
{
	public class FilesWebService : WebService
	{
		const int PACKAGE_SIZE = 1024;

		public override string ServiceURI
		{
			get
			{
				return "/files";
			}
		}

		public override void Handler(WebRequest req)
		{
			
		}

		//void WriteFileResponse(WebRequest req, FSFile file)
		//{
		//	Stream stream = file.OpenReadOnly();
		//	StringBuilder res = new StringBuilder();

		//	// TODO: Finish implementation!!!

		//	// Send reponse code and headers

		//	res.Append("HTTP/1.1 200 OK\r\n");
		//	res.Append("Content-Length: " + stream.Length + "\r\n\r\n");
		//	byte[] resBytes = Encoding.ASCII.GetBytes(res.ToString());
		//	res.Response.Write(resBytes, 0, resBytes.Length);

		//	while (true)
		//	{
		//		byte[] buf = new byte[PACKAGE_SIZE];
		//		int read = stream.Read(buf, 0, buf.Length);

		//		if (0 == read)
		//		{
		//			break;
		//		}

		//		req.Response.Write(buf, 0, read);
		//	}
		//}
	}
}
