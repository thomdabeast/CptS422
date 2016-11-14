using System;
using System.IO;
using System.Text;

namespace CS422
{
	public class FilesWebService : WebService
	{
		const int PACKAGE_SIZE = 1024;
        FileSys422 fileSystem;

        public FilesWebService(FileSys422 sys)
        {
            fileSystem = sys;
        }

		public override string ServiceURI
		{
			get
			{
				return "/files";
			}
		}

		string BuildDirHTML(Dir422 directory)
		{
			StringBuilder builder = new StringBuilder();

			// Styling
			builder.Append("<html>");
			builder.Append("<head><style>");
			builder.Append(" tr:nth-child(even) {background-color: #f2f2f2} ");
			builder.Append(" table { text-align : center; border: 2px solid black } ");
			builder.Append(" .center { position: absolute; width : 75%; left: 12.5% } ");
			builder.Append(" h1 { font-size: 4em } ");
			builder.Append(" a { font-size: 2em } ");
			builder.Append("</style></head>");
			builder.Append("<body style='text-align:center'><div class='center'>");
			builder.Append("<h1>Directories</h1>");

			// Get all directories
			builder.Append("<table width='100%'>");
			foreach (var dir in directory.GetDirs())
			{
				// Need to get rid of '**/{current_dir}/' in uri
				string uri = FullPath.Make(directory, dir.Name).Replace(fileSystem.GetRoot().Name + "/", "");

				builder.Append("<tr><td><a href='" + uri + "'>" + dir.Name + "</a></td></tr>");
			}
			builder.Append("</table>");

			builder.Append("<h1>Files</h1>");
			// Get all files
			builder.Append("<table width='100%'>");


			foreach (var file in directory.GetFiles())
			{
				// Need to get rid of '**/{current_dir}/' in uri
				string uri = FullPath.Make(directory, file.Name).Replace(fileSystem.GetRoot().Name + "/", "");

				builder.Append("<tr><td><a href='" + uri + "'>" + file.Name + "</a></td></tr>");
			}
			builder.Append("</table>");

			builder.Append("</div></body>");
			builder.Append("</html>");

			return builder.ToString();
		}

		public override void Handler(WebRequest req)
		{
			try
			{
				string[] path = req.URI.Split('/');

				// To handle '/files' request
				if (path.Length == 2)
				{
					req.WriteHTMLResponse(BuildDirHTML(fileSystem.GetRoot()));

					return;
				}

				// This handles '/files/'
				// The URI maps to an existing directory in the file system
				if (fileSystem.GetRoot().ContainsDir(path[path.Length - 1], true))
				{
					// Imidiately set the content-type to html since we are returning html code
					req.Headers["content-type"] = "text/html";

					Dir422 currentDir = fileSystem.GetRoot();
					for (int i = 2; i < path.Length; i++)
					{
						// Fix '/' at end of URI 
						if (path[i] != "")
						{
							currentDir = currentDir.GetDir(path[i]);
						}
					}

					req.WriteHTMLResponse(BuildDirHTML(currentDir));
				}
				// The URI maps to an existing file in the file system
				else if (fileSystem.GetRoot().ContainsFile(path[path.Length - 1], true))
				{
					// Get the file type and set the content-type to the file type
					string filename = path[path.Length - 1];
					int dot = filename.LastIndexOf('.');
					string type = (dot >= 0) ? filename.Substring(dot).ToLower() : "";

					switch (type)
					{
						case ".xml":
							req.Headers["content-type"] = "text/xml";
							break;
						case ".jpg":
							req.Headers["content-type"] = "image/jpg";
							break;
						case ".png":
							req.Headers["content-type"] = "image/png";
							break;
						case ".mp4":
							req.Headers["content-type"] = "video/mp4";
							break;
						case ".pdf":
							req.Headers["content-type"] = "application/pdf";
							break;
						default:
							req.Headers["content-type"] = "text/html";
							break;
					}

					Dir422 currentDir = fileSystem.GetRoot();

					// Don't include last element because thats the file name
					for (int i = 2; i < path.Length - 1; i++)
					{
						// Fix '/' at end of URI 
						if (path[i] != "")
						{
							currentDir = currentDir.GetDir(path[i]);
						}
					}

					File422 file = currentDir.GetFile(path[path.Length - 1]);

					// Get the file stuff
					int bytesRead;

					// Did the client send us a range header
					string rangeValue;
					if (req.Headers.TryGetValue("range", out rangeValue))
					{
						byte[] buffer;

						// Send the chunk they want. Only works for bytes=%d-%d
						rangeValue = rangeValue.Replace(" ", "");
						rangeValue = rangeValue.Remove(0, "bytes=".Length);
						string[] bytes = rangeValue.Split('-');
						int start = int.Parse(bytes[0]), end = int.Parse(bytes[1]), totalRead = 0;
						buffer = new byte[end-start];

						using (BufferedStream reader = new BufferedStream(file.OpenReadOnly()))
						{
							// Add header = content-range: bytes %d-%d/%d
							req.Headers["content-range"] = "bytes " + rangeValue + "/" + reader.Length;

							req.WritePreBody(206, buffer.Length);

							while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
							{
								req.WriteFile(buffer, totalRead, bytesRead);

								totalRead += bytesRead;
							}
						}
					}
					else
					{

						// Send the entire thing
						BufferedStream reader = new BufferedStream(file.OpenReadOnly());
						byte[] buffer = new byte[8 * 1024];

						req.WritePreBody(200, reader.Length);

						while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
						{
							req.WriteFile(buffer, 0, bytesRead);

						}

						reader.Close();
					}
				}
				else
				{
					// The URI maps to something that doesn’t exist in the file system
					req.WriteNotFoundResponse("<h1>404</h1><br><b>Content not found</b>");
				}
			}
			catch(Exception e)
			{
				return;
			}
		}
	}
}