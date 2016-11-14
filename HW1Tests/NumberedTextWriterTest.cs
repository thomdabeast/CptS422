using System;
using System.IO;
using System.Text;
using CS422;
using N

namespace HW1Tests
{
	[TestFixture()]
	public class NumberedTextWriterTest
	{
		class VariableWriter : TextWriter
		{
			public string Value
			{
				get; set;
			}

			public override Encoding Encoding
			{
				get
				{
					return Encoding.ASCII;
				}
			}

			public override void WriteLine(string format, params object[] arg)
			{
				Value = String.Format(format, arg);
			}
		}

		[Test()]
		public void NumberLineTest()
		{
			VariableWriter variableWriter = new VariableWriter();
			NumberedTextWriter numberedTextWriter = new NumberedTextWriter(variableWriter);
			string text = "Hello world!";
			int currentLine = numberedTextWriter.LineNumber;

			numberedTextWriter.WriteLine(text);

			Assert.AreEqual(variableWriter.Value, String.Format("{0}: {1}", currentLine, text));
		}

	}
}

