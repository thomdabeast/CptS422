using System;

namespace CS422
{
	class DemoService : WebService
	{
		private const string c_template =
			 "<html>This is the response to the request:<br>" +
			 "Method: {0}<br>Request-Target/URI: {1}<br>" +
			 "Request body size, in bytes: {2}<br><br>" +
			 "Student ID: {3}</html>";
		
		public DemoService()
		{
			
		}

		public override string ServiceURI
		{
			get
			{
				return "/";
			}
		}

		public override void Handler(WebRequest req)
		{
			req.WriteHTMLResponse(string.Format(c_template, req.Method, req.URI, req.Body.Length, "11337255"));
		}
	}
}
