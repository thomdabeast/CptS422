using System;
using System.IO;
using System.Text;

namespace CS422
{
	public class NumberedTextWriter : TextWriter
	{
		int lineNumber = 1;
		TextWriter textWriter;

		public NumberedTextWriter(TextWriter tw)
		{
			textWriter = tw;
			lineNumber = 1;
		}

		public NumberedTextWriter(TextWriter tw, int line) 
		{
			textWriter = tw;
			lineNumber = line;
		}

		public int LineNumber
		{
			get
			{
				return lineNumber;
			}
		}

		public override Encoding Encoding
		{
			get
			{
				return textWriter.Encoding;
			}
		}

		public override void Write(char value)
		{
			textWriter.Write(value);
		}

		public override void WriteLine(string value)
		{
			textWriter.WriteLine("{0}: {1}", (lineNumber++), value);
		}
	}
}

